using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

public class FolderEncryptor
{
    private const int SALT_SIZE_BYTES = 32; //256 bits
    private const int KEY_SIZE_BYTES = 32; //256 bits
    private const int IV_SIZE_BYTES = 16; //128 bits
    private const int MAX_PBKDF2_ITERATION = 600000; 
    public static void EncryptFolder(string folderPath, string password, string? keyFilePath = default)
    {
        // Validate the folder path
        if (!Directory.Exists(folderPath))
        {
            throw new DirectoryNotFoundException($"The folder '{folderPath}' was not found.");
        }
        if (string.IsNullOrEmpty(keyFilePath) == false && File.Exists(keyFilePath) == false)
        {
            Console.WriteLine($"Generating new file key '{keyFilePath}'...");
            GenerateKeyFile(keyFilePath);
        }

        // Generate a random salt
        byte[] salt = GenerateRandomSalt();

        // Create AES encryptor from password
        using (Aes aesAlg = Aes.Create())
        {
            Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(password, salt);
            aesAlg.Key = pdb.GetBytes(KEY_SIZE_BYTES); // AES-256
            aesAlg.IV = pdb.GetBytes(IV_SIZE_BYTES); // AES block size is 128 bits

            var (key, iv) = GenerateKeyAndIV(password, salt, keyFilePath);
            aesAlg.Key = key; // AES-256
            aesAlg.IV = iv; // AES block size is 128 bits

            // Compress the encrypted files into a single file with the .enc extension
            string compressedEncryptedFile = folderPath + ".enc";
            ZipFile.CreateFromDirectory(folderPath, compressedEncryptedFile);
            EncryptFile(compressedEncryptedFile, aesAlg, salt);
            Directory.Delete(folderPath, true);
        }
    }

    public static string DecryptFolder(string encryptedFileFullName, string password, string? decryptTargetPath
        , string? keyFilePath = default)
    {
        // Validate the encrypted file path
        if (!File.Exists(encryptedFileFullName))
        {
            throw new FileNotFoundException($"The encrypted file '{encryptedFileFullName}' was not found.");
        }

        string decryptedFileWithoutExt = Path.GetFileNameWithoutExtension(encryptedFileFullName);
        string decryptZipFileName = decryptedFileWithoutExt + "zip";

        DecryptFile(encryptedFileFullName, decryptZipFileName, password, keyFilePath);

        if (string.IsNullOrEmpty(decryptTargetPath) == true)
            decryptTargetPath = Directory.GetParent(encryptedFileFullName)?.FullName;
        // Decompress the .enc file
        string folderDecriptionPath = Path.Combine(decryptTargetPath, decryptedFileWithoutExt);
        ZipFile.ExtractToDirectory(decryptZipFileName, folderDecriptionPath);
        File.Delete(decryptZipFileName);
        File.Delete(encryptedFileFullName);
        return folderDecriptionPath;
    }

    private static void EncryptFile(string fileFullName, Aes aesAlg, byte[] salt)
    {
        // Read file bytes
        byte[] fileContent = File.ReadAllBytes(fileFullName);

        // Encrypt file content
        byte[] encryptedContent;
        using (ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV))
        {
            encryptedContent = PerformCryptography(fileContent, encryptor);
        }

        // Prepend the salt to the encrypted content
        byte[] encryptedContentWithSalt = new byte[salt.Length + encryptedContent.Length];
        Buffer.BlockCopy(salt, 0, encryptedContentWithSalt, 0, salt.Length);
        Buffer.BlockCopy(encryptedContent, 0, encryptedContentWithSalt, salt.Length, encryptedContent.Length);

        // Write encrypted content with salt to output file
        File.WriteAllBytes(fileFullName, encryptedContentWithSalt);
    }

    private static void DecryptFile(string inputFilePath, string outputFilePath, string password, string? keyFilePath = default)
    {
        // Read the encrypted content with salt from the file
        byte[] encryptedContentWithSalt = File.ReadAllBytes(inputFilePath);

        // Extract the salt
        byte[] salt = new byte[SALT_SIZE_BYTES]; // The length of the salt is known (256 bits)
        Buffer.BlockCopy(encryptedContentWithSalt, 0, salt, 0, salt.Length);

        // Extract the encrypted content
        byte[] encryptedContent = new byte[encryptedContentWithSalt.Length - salt.Length];
        Buffer.BlockCopy(encryptedContentWithSalt, salt.Length, encryptedContent, 0, encryptedContent.Length);

        // Create AES decryptor from password and salt
        using (Aes aesAlg = Aes.Create())
        {
            var (key, iv) = GenerateKeyAndIV(password, salt, keyFilePath);
            aesAlg.Key = key; // AES-256
            aesAlg.IV = iv; // AES block size is 128 bits

            // Decrypt file content
            byte[] decryptedContent;
            using (ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV))
            {
                decryptedContent = PerformCryptography(encryptedContent, decryptor);
            }

            // Write decrypted content to output file
            File.WriteAllBytes(outputFilePath, decryptedContent);
        }
    }

    private static byte[] PerformCryptography(byte[] data, ICryptoTransform cryptoTransform)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            using (CryptoStream cs = new CryptoStream(ms, cryptoTransform, CryptoStreamMode.Write))
            {
                cs.Write(data, 0, data.Length);
                cs.FlushFinalBlock();
            }
            return ms.ToArray();
        }
    }

    private static byte[] GenerateRandomSalt(int size = SALT_SIZE_BYTES)
    {
        byte[] data = new byte[size];
        using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
        {
            rng.GetBytes(data);
        }
        return data;
    }

    // Method to generate a key file with random data
    public static void GenerateKeyFile(string keyFilePath, int size = SALT_SIZE_BYTES)
    {
        var keyData = GenerateRandomSalt(size);

        // Write the random bytes to the key file
        File.WriteAllBytes(keyFilePath, keyData);
    }
    private static (byte[] key, byte[] iv) GenerateKeyAndIV(string password, byte[] salt, string? keyFilePath = default)
    {
        // Use the key file's contents as an additional factor in the key derivation process
        byte[] keyFileBytes = string.IsNullOrEmpty(keyFilePath) == false
                                    && File.Exists(keyFilePath)
                            ? File.ReadAllBytes(keyFilePath) : new byte[0];

        // Combine the salt and key file bytes to use as the salt parameter in the key derivation function
        byte[] combinedSalt = new byte[salt.Length + keyFileBytes.Length];
        Buffer.BlockCopy(salt, 0, combinedSalt, 0, salt.Length);
        Buffer.BlockCopy(keyFileBytes, 0, combinedSalt, salt.Length, keyFileBytes.Length);

        // Use Rfc2898DeriveBytes to generate the key and IV
        using (var keyGenerator = new Rfc2898DeriveBytes(password, combinedSalt, MAX_PBKDF2_ITERATION, HashAlgorithmName.SHA512))
        {
            return new() { key = keyGenerator.GetBytes(KEY_SIZE_BYTES), iv = keyGenerator.GetBytes(IV_SIZE_BYTES) };
        }
    }
}
