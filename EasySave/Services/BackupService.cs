using System;
using System.IO;
using System.Collections.Generic;
using EasySave.Models;
using EasyLog;

namespace EasySave.Services
{
    public class BackupService
    {
        // On rend la liste publique pour que le Program.cs puisse l'afficher
        public List<BackupJob> Jobs { get; set; }
        private Logger _logger;

        public BackupService()
        {
            Jobs = new List<BackupJob>();

            // Initialisation du logger
            string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            _logger = new Logger(logPath);
        }

        // Méthode pour ajouter un job (Max 5)
        public bool AddJob(BackupJob job)
        {
            if (Jobs.Count >= 5) return false; // Limite atteinte
            Jobs.Add(job);
            return true;
        }

        public void ExecuteJob(BackupJob job)
        {
            // Sécurités
            if (string.IsNullOrEmpty(job.SourceDirectory) || string.IsNullOrEmpty(job.TargetDirectory)) return;

            if (!Directory.Exists(job.SourceDirectory))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ERREUR] Source introuvable : {job.SourceDirectory}");
                Console.ResetColor();
                return;
            }

            // Création cible
            if (!Directory.Exists(job.TargetDirectory)) Directory.CreateDirectory(job.TargetDirectory);

            Console.WriteLine($"Traitement de : {job.Name}...");

            // Appel de la copie récursive
            CopyDirectory(job.SourceDirectory, job.TargetDirectory, job);
        }

        private void CopyDirectory(string sourceDir, string targetDir, BackupJob job)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDir);

            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(targetDir, file.Name);
                long startTime = DateTime.Now.Ticks;

                try
                {
                    file.CopyTo(targetFilePath, true); // Copie réelle
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur copie fichier: {ex.Message}");
                }

                long timeMs = (DateTime.Now.Ticks - startTime) / 10000;

                // Log via la DLL
                _logger.WriteLog(job.Name, file.FullName, targetFilePath, file.Length, timeMs);
                Console.WriteLine($" -> {file.Name} copié.");
            }

            foreach (DirectoryInfo subDir in dir.GetDirectories())
            {
                string newTargetDir = Path.Combine(targetDir, subDir.Name);
                CopyDirectory(subDir.FullName, newTargetDir, job);
            }
        }
    }
}