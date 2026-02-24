using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace EasySave.Services
{
    public class CryptoSoftService
    {
        private string cryptoSoftPath;

        public CryptoSoftService()
        {
            cryptoSoftPath = string.Empty;
        }

        // Sets the exact path to the CryptoSoft tool
        public void SetPath(string path)
        {
            cryptoSoftPath = path;
        }

        // Checks if a file should be encrypted based on its extension
        public bool IsEligible(string filePath, List<string> extensions)
        {
            // If the path is empty or there are no extensions, we don't encrypt
            if (string.IsNullOrEmpty(filePath) || extensions == null || extensions.Count == 0)
                return false;

            // Get the extension of the file, including the dot (example: ".txt")
            string fileExtension = Path.GetExtension(filePath);

            // Check if the extension is in the list, supporting both "txt" and ".txt" formats
            return extensions.Exists(e =>
                e.Equals(fileExtension, StringComparison.OrdinalIgnoreCase) ||
                ("." + e).Equals(fileExtension, StringComparison.OrdinalIgnoreCase));
        }

        // Encrypts the file using CryptoSoft and returns the time it took
        public long EncryptFile(string filePath)
        {
            // Stop if the tool or the file is missing
            if (!File.Exists(cryptoSoftPath)) return -1;
            if (!File.Exists(filePath)) return -1;

            try
            {
                // Start a timer to measure the encryption duration
                Stopwatch stopwatch = Stopwatch.StartNew();

                // Prepare the process to run CryptoSoft silently in the background
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = cryptoSoftPath,
                    // Send the file path twice to use the same file as source and destination
                    Arguments = $"\"{filePath}\" \"{filePath}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                // Run the process and wait until the encryption is done
                using (Process process = Process.Start(startInfo))
                {
                    process?.WaitForExit();
                }

                // Stop the timer and return the result in milliseconds
                stopwatch.Stop();
                return stopwatch.ElapsedMilliseconds;
            }
            catch
            {
                // Return -1 if an error happens during the process
                return -1;
            }
        }
    }
}