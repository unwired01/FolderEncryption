using System;
using System.IO;
using System.Text;
namespace FolderEncryption;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length != 2 || (args[0] != "-e" && args[0] != "-d"))
        {
            Console.WriteLine("FolderEncryptor -e|-d <folderPath>");
            Console.WriteLine("-e [FOLDER_NAME] encrypt folder (-e c:\\myfolder)");
            Console.WriteLine("-d [FILE_NAME] decrypt file to folder (-d file.enc)");
            return;
        }
        bool encrypt = args[0] == "-e";
        string folderPath = args[1];

        Console.Write("Enter your password: ");
        string password = ReadPassword();

        try
        {
            if (encrypt)
            {
                FolderEncryptor.EncryptFolder(folderPath, password);
                Console.WriteLine("Folder encrypted successfully.");
            }
            else
            {
                FolderEncryptor.DecryptFolder(folderPath, password);
                Console.WriteLine("Folder decrypted successfully.");
            }
        }
        catch(System.Security.Cryptography.CryptographicException ex)
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
}
