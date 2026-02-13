namespace EasyLog
{
    // Interface for the Logger (SOLID principle)
    public interface ILogger
    {
        void WriteLog(LogData data);
    }
}