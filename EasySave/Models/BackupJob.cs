namespace EasySave.Models
{
    /// <summary>
    /// Defines a backup job persisted in jobs.json.
    /// </summary>
    public class BackupJob
    {
        /// <summary>
        /// Gets or sets the user-defined job name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the source directory path to back up.
        /// </summary>
        public string? SourceDirectory { get; set; }

        /// <summary>
        /// Gets or sets the destination directory path.
        /// </summary>
        public string? TargetDirectory { get; set; }

        /// <summary>
        /// Gets or sets the backup strategy (Full or Differential).
        /// </summary>
        public BackupType Type { get; set; }

        /// <summary>
        /// Initializes an empty instance for JSON serialization.
        /// </summary>
        public BackupJob() { }

        /// <summary>
        /// Initializes a fully defined backup job.
        /// </summary>
        public BackupJob(string name, string source, string target, BackupType type)
        {
            Name = name;
            SourceDirectory = source;
            TargetDirectory = target;
            Type = type;
        }
    }
}