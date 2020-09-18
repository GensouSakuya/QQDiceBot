using System;
using System.Collections.Generic;
using System.Text;
using GensouSakuya.QQBot.Core.Base;

namespace GensouSakuya.QQBot.Core
{
    public class Core
    {
        public static void Init()
        {
            DataManager.Init();
            CommandCenter.ReloadManagers();
        }
    }
}
