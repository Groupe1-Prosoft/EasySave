namespace EasyLog
{
    // Interface for the Logger (SOLID principle)
    public interface ILogger
    {
        bool WriteLog(LogData data);
    }
}