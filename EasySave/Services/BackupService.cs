using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq; // Nécessaire pour les listes
using EasySave.Models;
using EasyLog;

namespace EasySave.Services
{
    public class BackupService
    {
        public List<BackupJob> Jobs { get; set; }
        private readonly ILogger _logger;

        private readonly string _jobsFilePath;
        private readonly string _stateFilePath; // Nouveau fichier state.json

        public BackupService()
        {
            string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EasySave");
            if (!Directory.Exists(appDataPath)) Directory.CreateDirectory(appDataPath);

            _jobsFilePath = Path.Combine(appDataPath, "jobs.json");
            _stateFilePath = Path.Combine(appDataPath, "state.json"); // Définition du chemin

            _logger = new EasyLog.Logger();
            LoadJobs();
        }

        // --- GESTION DES JOBS (CRUD) ---
        public bool AddJob(BackupJob job)
        {
            if (Jobs.Count >= 5) return false;
            Jobs.Add(job);
            SaveJobs();
            return true;
        }

        public bool DeleteJob(int index)
        {
            int realIndex = index - 1;
            if (realIndex >= 0 && realIndex < Jobs.Count)
            {
                Jobs.RemoveAt(realIndex);
                SaveJobs();
                return true;
            }
            return false;
        }

        private void SaveJobs()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText(_jobsFilePath, JsonSerializer.Serialize(Jobs, options));
            }
            catch (Exception ex) { Console.WriteLine($"Erreur save jobs: {ex.Message}"); }
        }

        private void LoadJobs()
        {
            try
            {
                if (File.Exists(_jobsFilePath))
                {
                    string json = File.ReadAllText(_jobsFilePath);
                    Jobs = JsonSerializer.Deserialize<List<BackupJob>>(json) ?? new List<BackupJob>();
                }
                else { Jobs = new List<BackupJob>(); }
            }
            catch { Jobs = new List<BackupJob>(); }
        }

        // --- EXÉCUTION & ÉTAT (STATE) ---

        public void ExecuteJob(BackupJob job)
        {
            if (!Directory.Exists(job.SourceDirectory))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ERREUR] Source introuvable : {job.SourceDirectory}");
                Console.ResetColor();
                return;
            }
            if (!Directory.Exists(job.TargetDirectory)) Directory.CreateDirectory(job.TargetDirectory);

            // 1. Initialisation de l'État (State)
            var state = new BackupState
            {
                JobName = job.Name,
                Timestamp = DateTime.Now,
                State = "ACTIF",
                SourceDirectory = job.SourceDirectory, // Ajout pour info
                TargetDirectory = job.TargetDirectory  // Ajout pour info
            };

            // 2. Calcul des Totaux (Fichiers et Taille)
            CalculateTotals(job.SourceDirectory, state);

            // Premier enregistrement de l'état (Début)
            UpdateStateFile(state);

            Console.WriteLine($"Traitement de : {job.Name} ({state.TotalFiles} fichiers)...");

            // 3. Lancement de la copie
            CopyDirectory(job.SourceDirectory, job.TargetDirectory, job, state);

            // 4. Fin du travail
            state.State = "NON ACTIF";
            state.CurrentSourceFile = "";
            state.CurrentTargetFile = "";

            state.Timestamp = DateTime.Now; // Update timestamp for job completion
            UpdateStateFile(state);
        }

        private void CalculateTotals(string path, BackupState state)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(path);
                // On compte tous les fichiers récursivement
                var files = dir.GetFiles("*", SearchOption.AllDirectories);

                state.TotalFiles = files.Length;
                state.FilesRemaining = files.Length;

                long size = 0;
                foreach (var f in files) size += f.Length;

                state.TotalSize = size;
                state.SizeRemaining = size;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur calcul taille: {ex.Message}");
            }
        }

        private void CopyDirectory(string sourceDir, string targetDir, BackupJob job, BackupState state)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDir);

            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(targetDir, file.Name);
                // Dans la méthode CopyDirectory, juste après : string targetFilePath = ...

                // --- DÉBUT DU BLOC À AJOUTER ---
                if (job.Type == BackupType.Differential && File.Exists(targetFilePath))
                {
                    FileInfo destFile = new FileInfo(targetFilePath);

                    // Si le fichier source est plus vieux ou égal à la destination
                    if (file.LastWriteTime <= destFile.LastWriteTime)
                    {
                        // On met à jour les compteurs (car le fichier est "traité" en étant ignoré)
                        state.FilesRemaining--;
                        state.SizeRemaining -= file.Length;
                        if (state.SizeRemaining < 0) state.SizeRemaining = 0;

                        //Update progression before skipping the file
                        state.Progression = state.TotalFiles > 0
                            ? (double)(state.TotalFiles - state.FilesRemaining) / state.TotalFiles * 100
                            : 0;

                        //Update of the state file to reflect the skipped file and progression
                        state.Timestamp = DateTime.Now;

                        // On sauvegarde l'état pour que la barre de progression avance
                        UpdateStateFile(state);

                        continue; // ON PASSE AU FICHIER SUIVANT
                    }
                }
                // --- FIN DU BLOC À AJOUTER ---
                long startTime = DateTime.Now.Ticks;

                // --- MISE A JOUR ETAT (Avant copie) ---
                state.CurrentSourceFile = file.FullName;
                state.CurrentTargetFile = targetFilePath;
                state.State = "ACTIF"; // On confirme qu'on est actif

                //Update of the state file to reflect the current file being copied
                state.Timestamp = DateTime.Now;

                UpdateStateFile(state); // Écriture JSON en temps réel


                //log write in the two cases (success or error) with time taken for the operation, and negative time if error
                try
                {
                    file.CopyTo(targetFilePath, true);
                    long timeMs = (DateTime.Now.Ticks - startTime) / 10000;

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
                    Console.WriteLine($" -> {file.Name} copié.");
                }
                catch (Exception ex)
                {
                    long timeMs = (DateTime.Now.Ticks - startTime) / 10000;

                    var logData = new LogData
                    {
                        Name = job.Name,
                        Source = file.FullName,
                        Target = targetFilePath,
                        Size = file.Length,
                        TransferTime = -timeMs,
                        Timestamp = DateTime.Now
                    };
                    _logger.WriteLog(logData);
                    Console.WriteLine($"Erreur copie : {ex.Message}");
                }

                state.FilesRemaining--;
                state.SizeRemaining -= file.Length;

                //Update progression after copying the file
                state.Progression = state.TotalFiles > 0
    ? (double)(state.TotalFiles - state.FilesRemaining) / state.TotalFiles * 100
    : 0;

                state.Timestamp = DateTime.Now; // Update timestamp for each file processed
                UpdateStateFile(state);
                if (state.SizeRemaining < 0) state.SizeRemaining = 0;
                // On évite les négatifs par sécurité
                if (state.SizeRemaining < 0) state.SizeRemaining = 0;




            }

            foreach (DirectoryInfo subDir in dir.GetDirectories())
            {
                string newTargetDir = Path.Combine(targetDir, subDir.Name);

                //create an under folder if it doesn't exist before copying files into it
                if (!Directory.Exists(newTargetDir)) Directory.CreateDirectory(newTargetDir);
                CopyDirectory(subDir.FullName, newTargetDir, job, state);
            }
        }

        // Méthode qui écrit ou met à jour le fichier state.json
        private void UpdateStateFile(BackupState currentState)
        {
            try
            {
                List<BackupState> states = new List<BackupState>();

                // Si le fichier existe, on le lit pour ne pas écraser les autres jobs (si on gérait le multi-thread)
                // Pour la console séquentielle, on écrase ou on met à jour la liste.
                if (File.Exists(_stateFilePath))
                {
                    string json = File.ReadAllText(_stateFilePath);
                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        states = JsonSerializer.Deserialize<List<BackupState>>(json) ?? new List<BackupState>();
                    }
                }

                // On cherche si le job existe déjà dans la liste
                var existingState = states.FirstOrDefault(s => s.JobName == currentState.JobName);
                if (existingState != null)
                {
                    // Mise à jour de l'entrée existante
                    states.Remove(existingState);
                }

                // On ajoute le nouvel état frais
                states.Add(currentState);

                var options = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText(_stateFilePath, JsonSerializer.Serialize(states, options));
            }
            catch
            {
                // On ignore les erreurs d'écriture d'état pour ne pas bloquer la copie
            }
        }
    }
}