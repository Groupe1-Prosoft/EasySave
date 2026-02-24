using System;
using System.IO;
using System.Threading;

class Program
{
    // Unique name to identify the application across the system
    private static readonly string MutexName = "Global\\CryptoSoft_ProSoft_Mutex";

    static int Main(string[] args)
    {
        // Ask the system if we can start the application
        using (Mutex mutex = new Mutex(true, MutexName, out bool createdNew))
        {
            // If another instance is already running, stop here
            if (!createdNew)
            {
                Console.WriteLine("Error: Another instance of CryptoSoft is already running.");
                return -1;
            }

            try
            {
                // Check if source and destination paths are provided
                if (args.Length < 2)
                {
                    Console.WriteLine("Usage: CryptoSoft.exe <source_path> <destination_path>");
                    return -1;
                }

                // Get file paths and encryption key
                string sourcePath = args[0];
                string destPath = args[1];
                string key = "PROSOFT_KEY";

                // Read the file content and the key as bytes
                byte[] fileBytes = File.ReadAllBytes(sourcePath);
                byte[] keyBytes = System.Text.Encoding.ASCII.GetBytes(key);

                // Encrypt or decrypt the file using XOR operation
                for (int i = 0; i < fileBytes.Length; i++)
                {
                    fileBytes[i] = (byte)(fileBytes[i] ^ keyBytes[i % keyBytes.Length]);
                }

                // Save the modified file to the destination path
                File.WriteAllBytes(destPath, fileBytes);

                return 0;
            }
            catch (Exception ex)
            {
                // Handle any error that occurs during the process
                Console.WriteLine("Error: " + ex.Message);
                return -1;
            }
            finally
            {
                // Always release the lock so the next backup can use CryptoSoft
                mutex.ReleaseMutex();
            }
        }
    }
}