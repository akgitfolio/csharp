using System;
using System.IO;
using System.Security.Cryptography;

class FileEncrypter
{
    private static readonly byte[] Salt = new byte[] { 0x26, 0xdc, 0xff, 0x00, 0xad, 0xed, 0x7a, 0xee, 0xc5, 0xfe, 0x07, 0xaf, 0x4d, 0x08, 0x22, 0x3c };

    public static void EncryptFile(string inputFile, string outputFile, string password)
    {
        using (var aes = Aes.Create())
        {
            aes.KeySize = 256;
            aes.BlockSize = 128;
            var key = new Rfc2898DeriveBytes(password, Salt, 50000);
            aes.Key = key.GetBytes(aes.KeySize / 8);
            aes.IV = key.GetBytes(aes.BlockSize / 8);

            using (var inputFileStream = File.OpenRead(inputFile))
            using (var outputFileStream = File.Create(outputFile))
            using (var cryptoStream = new CryptoStream(outputFileStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
            {
                inputFileStream.CopyTo(cryptoStream);
            }
        }
    }

    public static void DecryptFile(string inputFile, string outputFile, string password)
    {
        using (var aes = Aes.Create())
        {
            aes.KeySize = 256;
            aes.BlockSize = 128;
            var key = new Rfc2898DeriveBytes(password, Salt, 50000);
            aes.Key = key.GetBytes(aes.KeySize / 8);
            aes.IV = key.GetBytes(aes.BlockSize / 8);

            using (var inputFileStream = File.OpenRead(inputFile))
            using (var outputFileStream = File.Create(outputFile))
            using (var cryptoStream = new CryptoStream(inputFileStream, aes.CreateDecryptor(), CryptoStreamMode.Read))
            {
                cryptoStream.CopyTo(outputFileStream);
            }
        }
    }

    static void Main(string[] args)
    {
        string projectDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;

        Console.Write("Enter 'e' to encrypt or 'd' to decrypt: ");
        var mode = Console.ReadLine();

        Console.Write("Enter input file path: ");
        var inputFile = Console.ReadLine();

        Console.Write("Enter output file path: ");
        var outputFile = Console.ReadLine();

        Console.Write("Enter password: ");
        var password = Console.ReadLine();

        if (mode == "e")
        {
            EncryptFile($"{projectDirectory}/{inputFile}", $"{projectDirectory}/{outputFile}", password);
            Console.WriteLine("File encrypted successfully.");
        }
        else if (mode == "d")
        {
            DecryptFile($"{projectDirectory}/{inputFile}", $"{projectDirectory}/{outputFile}", password);
            Console.WriteLine("File decrypted successfully.");
        }
        else
        {
            Console.WriteLine("Invalid mode selected.");
        }
    }
}
