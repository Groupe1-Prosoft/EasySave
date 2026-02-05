namespace EasySave.Models
{
    public class BackupJob
    {
        // The name of the backup job
        public string? Name { get; set; }

        // Path to the source directory
        public string? SourceDirectory { get; set; }

        // Path to the destination directory
        public string? TargetDirectory { get; set; }

        // Type of backup (Full or Differential)
        public BackupType Type { get; set; }

        // Empty constructor needed for JSON serialization
        public BackupJob() { }

        // Constructor to initialize the job
        public BackupJob(string name, string source, string target, BackupType type)
        {
            Name = name;
            SourceDirectory = source;
            TargetDirectory = target;
            Type = type;
        }
    }
}