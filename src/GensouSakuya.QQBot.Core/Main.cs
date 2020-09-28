using System;
using System.Collections.Generic;
using System.Text;
using GensouSakuya.QQBot.Core.Base;

namespace GensouSakuya.QQBot.Core
{
    public class Main
    {
        public static void Init()
        {
            DataManager.Init();
            CommandCenter.ReloadManagers();
        }
    }
}
