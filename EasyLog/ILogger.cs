namespace EasyLog
{
    /// <summary>
    /// Defines a contract for log persistence.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Writes a log entry to the target storage.
        /// </summary>
        bool WriteLog(LogData data);
    }
}