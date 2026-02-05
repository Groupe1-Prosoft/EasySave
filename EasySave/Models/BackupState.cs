using System;

namespace EasySave.Models
{
    public class BackupState
    {
        public string? JobName { get; set; }
        public DateTime Timestamp { get; set; }
        public string? State { get; set; }

        public string? SourceDirectory { get; set; }
        public string? TargetDirectory { get; set; }

        public int TotalFiles { get; set; }
        public long TotalSize { get; set; }
        public int FilesRemaining { get; set; }
        public long SizeRemaining { get; set; }

        public double Progression { get; set; }

        public string? CurrentSourceFile { get; set; }
        public string? CurrentTargetFile { get; set; }

        public BackupState()
        {
            Timestamp = DateTime.Now;
            State = "NON ACTIF";
            JobName = "";
            CurrentSourceFile = "";
            CurrentTargetFile = "";
        }
    }
}