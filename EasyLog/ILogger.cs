namespace EasyLog
{
    /// <summary>
    /// Defines a contract for log persistence.
    /// </summary>
    public interface ILogger
    {
        void WriteLog(LogData data);
    }
}