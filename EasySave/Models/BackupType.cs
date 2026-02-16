namespace EasySave.Models
{
    /// <summary>
    /// Enumerates supported backup strategies.
    /// </summary>
    public enum BackupType
    {
        /// <summary>
        /// Full copy of all files.
        /// </summary>
        Full,

        /// <summary>
        /// Copies only files newer than the destination.
        /// </summary>
        Differential
    }
}