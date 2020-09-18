namespace GensouSakuya.QQBot.Core.PlatformModel
{
    public class Log
    {
        public Log(string message, LogLevel level)
        {
            Message = message;
            Level = level;
        }

        public Log() { }

        public string Message { get; set; }
        public LogLevel Level { get; set; }

    }

    public enum LogLevel
    {
        Debug,
        Info,
        Error,
        Fatal
    }
}
