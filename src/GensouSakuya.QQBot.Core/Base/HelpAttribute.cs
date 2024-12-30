using System;

namespace GensouSakuya.QQBot.Core.Base
{
    public class HelpAttribute : Attribute
    {
        public string Describe { get; }
        public HelpAttribute(string describe)
        {
            Describe = describe;
        }
    }
}
