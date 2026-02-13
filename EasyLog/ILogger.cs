namespace EasyLog
{
    /// <summary>
    /// Defines a contract for log persistence.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Writes a single log entry to the configured output.
        /// </summary>
        bool WriteLog(LogData data);
    }
}