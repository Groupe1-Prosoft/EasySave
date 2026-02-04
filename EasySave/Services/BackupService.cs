using System;
using System.IO;
using System.Collections.Generic;
using EasySave.Models;
using EasyLog; // Indispensable pour parler à ta DLL

namespace EasySave.Services
{
    public class BackupService
    {
        public List<BackupJob> Jobs { get; set; }
        private readonly ILogger _logger; // On utilise l'interface SOLID

        public BackupService()
        {
            Jobs = new List<BackupJob>();
            _logger = new EasyLog.Logger(); // On instancie le logger de la DLL
        }

        // Ajoute un job (Max 5 selon le cahier des charges)
        public bool AddJob(BackupJob job)
        {
            if (Jobs.Count >= 5) return false;
            Jobs.Add(job);
            return true;
        }

        public void ExecuteJob(BackupJob job)
        {
            if (string.IsNullOrEmpty(job.SourceDirectory) || string.IsNullOrEmpty(job.TargetDirectory)) return;

            if (!Directory.Exists(job.SourceDirectory))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ERREUR] Source introuvable : {job.SourceDirectory}");
                Console.ResetColor();
                return;
            }

            if (!Directory.Exists(job.TargetDirectory)) Directory.CreateDirectory(job.TargetDirectory);

            Console.WriteLine($"Traitement de : {job.Name}...");

            // Appel de la copie récursive
            CopyDirectory(job.SourceDirectory, job.TargetDirectory, job);
        }

        private void CopyDirectory(string sourceDir, string targetDir, BackupJob job)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDir);

            // 1. Copie des fichiers
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(targetDir, file.Name);
                long startTime = DateTime.Now.Ticks;

                try
                {
                    file.CopyTo(targetFilePath, true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur copie fichier: {ex.Message}");
                    continue; // Passe au fichier suivant en cas d'erreur
                }

                // Calcul du temps en ms
                long timeMs = (DateTime.Now.Ticks - startTime) / 10000;

                // --- CRÉATION DE L'OBJET LOG (Lien avec ta DLL) ---
                var logData = new LogData
                {
                    Name = job.Name,
                    Source = file.FullName,
                    Target = targetFilePath,
                    Size = file.Length,
                    TransferTime = timeMs,
                    Timestamp = DateTime.Now
                };

                // Écriture via la DLL
                _logger.WriteLog(logData);

                Console.WriteLine($" -> {file.Name} copié ({timeMs}ms).");
            }

            // 2. Récursion pour les sous-dossiers
            foreach (DirectoryInfo subDir in dir.GetDirectories())
            {
                string newTargetDir = Path.Combine(targetDir, subDir.Name);
                CopyDirectory(subDir.FullName, newTargetDir, job);
            }
        }
    }
}