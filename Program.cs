using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace CryptoSoft
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: CryptoSoft.exe <input file list> <destination directory>");
                return -1; // error for "invalid argument"
            }

            string inputFileListPath = args[0];
            string destinationDirectoryPath = args[1];
            string keyFilePath = "config.txt"; 

            try
            {
                // Reading key
                string key = File.ReadAllText(keyFilePath, Encoding.UTF8);
                if (string.IsNullOrEmpty(key))
                {
                    Console.WriteLine("Encryption key is missing in the config file.");
                    return -2; // Key is missing
                }

                // Destination directory exist
                if (!Directory.Exists(destinationDirectoryPath))
                {
                    Directory.CreateDirectory(destinationDirectoryPath);
                }

                // reading path files to encrypt from source file
                string[] filePaths = File.ReadAllLines(inputFileListPath, Encoding.UTF8);

                // Chronometer to encryption time
                Stopwatch globalStopwatch = new Stopwatch();
                globalStopwatch.Start();

                foreach (string sourceFilePath in filePaths)
                {
                    string cleanedPath = sourceFilePath.Trim();
                    // Verify if source file exist
                    if (!File.Exists(sourceFilePath))
                    {
                        Console.WriteLine($"File not found: {sourceFilePath}");
                        continue; // Next file
                    }

                    string destinationFilePath = Path.Combine(destinationDirectoryPath, Path.GetFileName(sourceFilePath) + ".encrypted");

                    // Encrypt File
                    EncryptFile(sourceFilePath, destinationFilePath, key);
                }

                globalStopwatch.Stop();
                Console.WriteLine($"All files processed successfully in {globalStopwatch.ElapsedMilliseconds} ms.");

                // Return 0 to inform there's no error
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return -3; // Error during file treatment
            }
        }

        private static void EncryptFile(string sourceFilePath, string destinationFilePath, string key)
        {
            // Convert key in table
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);

            // Use FileStream to read and write the files
            using (FileStream sourceStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read))
            using (FileStream destinationStream = new FileStream(destinationFilePath, FileMode.Create, FileAccess.Write))
            {
                int bytesRead;
                byte[] buffer = new byte[4096]; // Buffer size
                int keyIndex = 0;

                while ((bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    // Buffer content encryption
                    for (int i = 0; i < bytesRead; i++)
                    {
                        buffer[i] = (byte)(buffer[i] ^ keyBytes[keyIndex % keyBytes.Length]);
                        keyIndex++;
                    }

                    destinationStream.Write(buffer, 0, bytesRead);
                }
            }

            Console.WriteLine($"File encrypted: {sourceFilePath} -> {destinationFilePath}");
        }

    }
}
