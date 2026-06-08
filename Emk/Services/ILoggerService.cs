namespace Emk.Services
{
    public interface ILoggerService
    {
        void Error(string msg);
        void Info(string msg);
        void Warning(string msg);
    }

    public enum LoggerLevel
    {
        Error,
        Warning,
        Info
    }
}