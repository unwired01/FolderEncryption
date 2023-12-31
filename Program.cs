using System;
using System.Diagnostics;
using System.IO;
using System.Text;
namespace FolderEncryption;

class Program
{
    static void Main(string[] args)
    {
        var targetDecryptPath = string.Empty;
        var password = string.Empty;
        try
        {
            string optionText = string.Empty;
            string shaAlogorithem = string.Empty;
            string iteration = string.Empty;
            do
            {
                Console.Write("Choose on option (E)ncryption or (D)ecryption or (C)onfiguration or 'exit': ");
                optionText = Console.ReadLine().ToLower().Trim();
                if (optionText == "e")
                {
                    Console.Write("Enter the folder path to encrypt: ");
                    string folderPath = Console.ReadLine();
                    Console.Write("Enter your password: ");
                    password = ReadPassword();
                    Console.Write("Confirm the password: ");
                    string confirmPassword = ReadPassword();
                    if (password != confirmPassword)
                        Console.WriteLine("Passwords do not match.");
                    else
                    {
                        Console.Write("Enter the key file path (optional): ");
                        string keyFilePath = Console.ReadLine();
                        FolderEncryptor.EncryptFolder(folderPath, password, keyFilePath);
                        Console.WriteLine("Folder encrypted successfully.");
                    }
                }
                else if (optionText == "d")
                {
                    Console.Write("Enter file name to decrypt: ");
                    string fileName = Console.ReadLine();
                    Console.Write("Enter destination folder for extraction (optional; if not provided, the parent folder will be used): ");
                    targetDecryptPath = Console.ReadLine();
                    Console.Write("Enter your password: ");
                    password = ReadPassword();
                    Console.Write("Enter the key file path (optional): ");
                    string keyFilePath = Console.ReadLine();
                    string folderPath = FolderEncryptor.DecryptFolder(fileName, password, targetDecryptPath, keyFilePath);
                    Console.WriteLine("Folder decrypted successfully.");
                    OpenDecryptFolder(folderPath);
                }
                else if (optionText == "c")
                {
                    Console.Write($"Enter HashAlgorithm (MD5/SHA1/SHA256/SHA384/SHA512) or blank (current: {FolderEncryptor.GetSHA()}): ");
                    shaAlogorithem = Console.ReadLine().Trim().ToUpper();
                    FolderEncryptor.SetSHA(shaAlogorithem );

                    Console.Write($"Enter iteration (current: {FolderEncryptor.GetIteration()}): ");
                    iteration = Console.ReadLine().Trim().ToUpper();
                    FolderEncryptor.SetIteration(iteration);
                }
                else if (optionText != "exit")
                {
                    Console.WriteLine("Invalid option (E|D|exit)");
                }
                else
                    return;
            } while (true);
        }
        catch (System.Security.Cryptography.CryptographicException ex)
        {
            Console.WriteLine($"********A Crypto error occurred, check your password********");
            Console.WriteLine($"{ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex}");
        }
    }
    private static string ReadPassword()
    {
        StringBuilder password = new StringBuilder();
        while (true)
        {
            ConsoleKeyInfo key = Console.ReadKey(true);
            if (key.Key == ConsoleKey.Enter)
            {
                break;
            }
            if (key.Key == ConsoleKey.Backspace && password.Length > 0)
            {
                password.Remove(password.Length - 1, 1);
                Console.Write("\b \b");
            }
            else if (key.KeyChar != '\u0000') // Ignore non-character keys
            {
                password.Append(key.KeyChar);
                Console.Write("*");
            }
        }
        Console.WriteLine();
        return password.ToString();
    }

    private static void OpenDecryptFolder(string folderPath)
    {
        ProcessStartInfo processStartInfo = new ProcessStartInfo
        {
            FileName = "explorer.exe",
            Arguments = folderPath,
            UseShellExecute = true
        };

        try
        {
            using (Process process = Process.Start(processStartInfo))
            {
                Console.WriteLine($"Opened {folderPath} in Windows Explorer.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}
