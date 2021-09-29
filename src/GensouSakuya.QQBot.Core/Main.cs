using System;
using GensouSakuya.QQBot.Core.Base;

namespace GensouSakuya.QQBot.Core
{
    public class Main
    {
        private static readonly Logger _logger = Logger.GetLogger<Main>();

        public static void Init()
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledException;
            DataManager.Init();
            CommandCenter.ReloadManagers();
            _logger.Info("bot is started");
        }

        private static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _logger.Error(e?.ExceptionObject as Exception, "Unhandled error");
        }
    }
}
