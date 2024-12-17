using Serilog;
using System;
using System.IO;

namespace GensouSakuya.QQBot.Core
{
    public class Logger
    {
        static Logger()
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Debug()
                .WriteTo.Console(
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File(Path.Combine("logs", "log-.txt"), rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                    shared: true, rollOnFileSizeLimit:true, fileSizeLimitBytes: 10 * 1024 * 1024)
                .CreateLogger();
        }
        public static Logger GetLogger<T>()
        {
            return new Logger(Log.Logger.ForContext<T>());
        }

        private ILogger _logger;
        private Logger(ILogger logger)
        {
            _logger = logger;
        }

        public void Info(string message)
        {
            _logger.Information(message);
        }

        public void Info(string messageTemplate, params object[] args)
        {
            _logger.Information(messageTemplate, args);
        }

        public void Error(string message)
        {
            _logger.Error(message);
        }

        public void Error(Exception e, string message)
        {
            _logger.Error(e, message);
        }

        public void Error(Exception e, string messageTemplate, params object[] args)
        {
            _logger.Error(e, messageTemplate, args);
        }

        public void Debug(string message)
        {
            _logger.Debug(message);
        }
        public void Debug(string messageTemplate, params object[] args)
        {
            _logger.Debug(messageTemplate, args);
        }

        public void Fatal(string message)
        {
            _logger.Fatal(message);
        }
    }
}
