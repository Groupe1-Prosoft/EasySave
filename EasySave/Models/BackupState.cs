using System;

namespace EasySave.Models
{
    public class BackupState
    {
        public string? JobName { get; set; } // Ajout du ?
        public DateTime Timestamp { get; set; }
        public string? State { get; set; }   // Ajout du ?

        public int TotalFiles { get; set; }
        public long TotalSize { get; set; }
        public int FilesRemaining { get; set; }
        public long SizeRemaining { get; set; }

        public string? CurrentSourceFile { get; set; } // Ajout du ?
        public string? CurrentTargetFile { get; set; } // Ajout du ?

        public BackupState()
        {
            Timestamp = DateTime.Now;
            State = "NON ACTIF";
            // On initialise les chaines pour éviter les avertissements
            JobName = "";
            CurrentSourceFile = "";
            CurrentTargetFile = "";
        }
    }
}