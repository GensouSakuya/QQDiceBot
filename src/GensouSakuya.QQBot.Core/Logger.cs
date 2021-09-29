using GensouSakuya.QQBot.Core.Base;
using Serilog;
using System;
using System.IO;

namespace GensouSakuya.QQBot.Core
{
    internal class Logger
    {
        static Logger()
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Debug()
                .WriteTo.Console(
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File(Path.Combine(Config.LogPath, "log-.txt"), rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                    shared: true)
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

        public void Error(Exception e, string message)
        {
            _logger.Error(e, message);
        }

        public void Debug(string message)
        {
            _logger.Debug(message);
        }
    }
}
