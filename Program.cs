using System;
using System.IO;
using System.Text;
namespace FolderEncryption;

class Program
{
    static void Main(string[] args)
    {
        var targetDecryptPath = string.Empty;
        var password = string.Empty;
        Console.Write("Choose on option (E)ncryption or (D)ecryption: ");
        string optionText = Console.ReadLine().ToLower();
        try
        {
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
                    FolderEncryptor.EncryptFolder(folderPath, password);
                    Console.WriteLine("Folder encrypted successfully.");
                }
            }
            else if (optionText == "d")
            {
                Console.Write("Enter file name to decrypt: ");
                string fileName = Console.ReadLine();
                Console.Write("Enter target folder to extract (Optional, otherwise find it on run-folder): ");
                targetDecryptPath = Console.ReadLine();
                Console.Write("Enter your password: ");
                password = ReadPassword();
                FolderEncryptor.DecryptFolder(fileName, password, targetDecryptPath);
                Console.WriteLine("Folder decrypted successfully.");

            }
            else
            {
                Console.WriteLine("Invalid option (E|D)");
            }
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

        Console.WriteLine("Press any key to exit");
        Console.ReadKey();
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
