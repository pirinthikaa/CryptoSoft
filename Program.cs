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
                return -1; // Code d'erreur pour "arguments invalides"
            }

            string inputFileListPath = args[0];
            string destinationDirectoryPath = args[1];
            string keyFilePath = "config.txt"; // Chemin vers le fichier de configuration contenant la clé

            try
            {
                // Lecture de la clé de chiffrement
                string key = File.ReadAllText(keyFilePath, Encoding.UTF8);
                if (string.IsNullOrEmpty(key))
                {
                    Console.WriteLine("Encryption key is missing in the config file.");
                    return -2; // Code d'erreur pour "clé de chiffrement manquante"
                }

                // Vérification que le répertoire de destination existe, sinon création
                if (!Directory.Exists(destinationDirectoryPath))
                {
                    Directory.CreateDirectory(destinationDirectoryPath);
                }

                // Lecture des chemins des fichiers à chiffrer depuis le fichier d'entrée
                //string[] filePaths = File.ReadAllLines(inputFileListPath);
                string[] filePaths = File.ReadAllLines(inputFileListPath, Encoding.UTF8);

                // Initialisation et démarrage du chronomètre global
                Stopwatch globalStopwatch = new Stopwatch();
                globalStopwatch.Start();

                foreach (string sourceFilePath in filePaths)
                {
                    string cleanedPath = sourceFilePath.Trim();
                    // Vérifie si le fichier source existe
                    if (!File.Exists(sourceFilePath))
                    {
                        Console.WriteLine($"File not found: {sourceFilePath}");
                        continue; // Passe au fichier suivant
                    }

                    string destinationFilePath = Path.Combine(destinationDirectoryPath, Path.GetFileName(sourceFilePath) + ".encrypted");

                    // Chiffrement du fichier
                    EncryptFile(sourceFilePath, destinationFilePath, key);
                }

                globalStopwatch.Stop();
                Console.WriteLine($"All files processed successfully in {globalStopwatch.ElapsedMilliseconds} ms.");

                // Retourne 0 pour indiquer que le programme s'est exécuté sans erreur
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return -3; // Code d'erreur pour "erreur lors du traitement du fichier"
            }
        }

        private static void EncryptFile(string sourceFilePath, string destinationFilePath, string key)
        {
            // Convertit la clé en tableau d'octets
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);

            // Utilisation de FileStream pour lire et écrire les fichiers
            using (FileStream sourceStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read))
            using (FileStream destinationStream = new FileStream(destinationFilePath, FileMode.Create, FileAccess.Write))
            {
                int bytesRead;
                byte[] buffer = new byte[4096]; // Taille du buffer ajustable selon les besoins
                int keyIndex = 0;

                while ((bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    // Chiffrement du contenu du buffer
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
