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

        public void SetPath(string path)
        {
            cryptoSoftPath = path;
        }

        /// <summary>
        /// Checks if a file is eligible for encryption based on its extension.
        /// </summary>
        public bool IsEligible(string filePath, List<string> extensions)
        {
            if (string.IsNullOrEmpty(filePath) || extensions == null || extensions.Count == 0)
                return false;

            string fileExtension = Path.GetExtension(filePath).TrimStart('.');
            // Check if extension exists in the list (case insensitive)
            return extensions.Exists(e => e.Equals(fileExtension, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Encrypts the file at the given path using CryptoSoft.
        /// Returns the encryption time in milliseconds, or -1 if failed.
        /// </summary>
        public long EncryptFile(string filePath)
        {
            if (!File.Exists(cryptoSoftPath)) return -1;
            if (!File.Exists(filePath)) return -1;

            try
            {
                Stopwatch stopwatch = Stopwatch.StartNew();

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = cryptoSoftPath,
                    Arguments = $"\"{filePath}\"", // Encrypt in place (single argument implies target file)
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(startInfo))
                {
                    process.WaitForExit();
                }

                stopwatch.Stop();
                return stopwatch.ElapsedMilliseconds;
            }
            catch
            {
                return -1;
            }
        }
    }
}