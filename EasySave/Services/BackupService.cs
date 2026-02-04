using System;
using System.IO;
using System.Collections.Generic;
using EasySave.Models;
using EasyLog;

namespace EasySave.Services
{
    public class BackupService
    {
        public List<BackupJob> Jobs { get; set; }
        private readonly ILogger _logger;

        public BackupService()
        {
            Jobs = new List<BackupJob>();
            // Attention : Assure-toi que ta DLL EasyLog est bien référencée
            _logger = new EasyLog.Logger();
        }

        // Ajoute un job (Max 5)
        public bool AddJob(BackupJob job)
        {
            if (Jobs.Count >= 5) return false;
            Jobs.Add(job);
            return true;
        }

        public void ExecuteJob(BackupJob job)
        {
            // Sécurité pour éviter les plantages si les chemins sont vides
            if (string.IsNullOrWhiteSpace(job.SourceDirectory) || string.IsNullOrWhiteSpace(job.TargetDirectory))
            {
                Console.WriteLine($"[ERREUR] Chemins invalides pour le job {job.Name}");
                return;
            }

            if (!Directory.Exists(job.SourceDirectory))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ERREUR] Source introuvable : {job.SourceDirectory}");
                Console.ResetColor();
                return;
            }

            // Création du dossier cible s'il n'existe pas
            if (!Directory.Exists(job.TargetDirectory)) Directory.CreateDirectory(job.TargetDirectory);

            Console.WriteLine($"Traitement de : {job.Name} [{job.Type}]...");

            // Appel de la copie récursive
            CopyDirectory(job.SourceDirectory, job.TargetDirectory, job);

            Console.WriteLine("--- Fin du job ---");
        }

        private void CopyDirectory(string sourceDir, string targetDir, BackupJob job)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDir);

            // Si le dossier source n'existe pas (sécurité supplémentaire pour la récursion)
            if (!dir.Exists) return;

            // Si le dossier cible n'existe pas pour ce sous-dossier, on le crée
            Directory.CreateDirectory(targetDir);

            // 1. Copie des fichiers
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(targetDir, file.Name);

                // --- DÉBUT LOGIQUE DIFFÉRENTIELLE ---
                if (job.Type == BackupType.Differential)
                {
                    if (File.Exists(targetFilePath))
                    {
                        // On compare la date de modification
                        FileInfo destFile = new FileInfo(targetFilePath);

                        // Si le fichier source est plus vieux ou égal au fichier de destination, on ne fait rien
                        if (file.LastWriteTime <= destFile.LastWriteTime)
                        {
                            // On passe au fichier suivant (continue)
                            continue;
                        }
                    }
                }
                // --- FIN LOGIQUE DIFFÉRENTIELLE ---

                long startTime = DateTime.Now.Ticks;

                try
                {
                    // Le "true" permet d'écraser le fichier s'il existe (nécessaire pour la mise à jour)
                    file.CopyTo(targetFilePath, true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur copie fichier: {ex.Message}");
                    continue;
                }

                // Calcul du temps en ms
                long timeMs = (DateTime.Now.Ticks - startTime) / 10000;

                // --- LOGGING ---
                var logData = new LogData
                {
                    Name = job.Name,
                    Source = file.FullName,
                    Target = targetFilePath,
                    Size = file.Length,
                    TransferTime = timeMs,
                    Timestamp = DateTime.Now
                };

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