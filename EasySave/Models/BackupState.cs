using System;

namespace EasySave.Models
{
    /// <summary>
    /// Represents runtime progress data stored in state.json.
    /// </summary>
    public class BackupState
    {
        /// <summary>
        /// Gets or sets the backup job name.
        /// </summary>
        public string? JobName { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of the last state update.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the job state (e.g., ACTIF, NON ACTIF).
        /// </summary>
        public string? State { get; set; }

        /// <summary>
        /// Gets or sets the source directory for context.
        /// </summary>
        public string? SourceDirectory { get; set; }

        /// <summary>
        /// Gets or sets the target directory for context.
        /// </summary>
        public string? TargetDirectory { get; set; }

        /// <summary>
        /// Gets or sets the total number of eligible files.
        /// </summary>
        public int TotalFiles { get; set; }

        /// <summary>
        /// Gets or sets the total size in bytes.
        /// </summary>
        public long TotalSize { get; set; }

        /// <summary>
        /// Gets or sets the remaining files count.
        /// </summary>
        public int FilesRemaining { get; set; }

        /// <summary>
        /// Gets or sets the remaining size in bytes.
        /// </summary>
        public long SizeRemaining { get; set; }

        /// <summary>
        /// Gets or sets the progression percentage.
        /// </summary>
        public double Progression { get; set; }

        /// <summary>
        /// Gets or sets the current source file being processed.
        /// </summary>
        public string? CurrentSourceFile { get; set; }

        /// <summary>
        /// Gets or sets the current target file being processed.
        /// </summary>
        public string? CurrentTargetFile { get; set; }

        /// <summary>
        /// Initializes a default inactive state.
        /// </summary>
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