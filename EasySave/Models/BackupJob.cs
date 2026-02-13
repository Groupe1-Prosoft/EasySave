using System.IO;

namespace EasySave.Models
{
    /// <summary>
    /// Defines a backup job persisted in configuration.
    /// </summary>
    public class BackupJob
    {
        /// <summary>
        /// Gets or sets the job identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the job name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the source directory.
        /// </summary>
        public string? SourceDir { get; set; }

        /// <summary>
        /// Gets or sets the target directory.
        /// </summary>
        public string? TargetDir { get; set; }

        /// <summary>
        /// Gets or sets the backup type.
        /// </summary>
        public BackupType Type { get; set; }

        /// <summary>
        /// Validates required job fields.
        /// </summary>
        public bool Validate()
        {
            return !string.IsNullOrWhiteSpace(Name)
                && !string.IsNullOrWhiteSpace(SourceDir)
                && !string.IsNullOrWhiteSpace(TargetDir)
                && Directory.Exists(SourceDir);
        }
    }
}