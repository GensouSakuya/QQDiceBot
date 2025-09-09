using System;

namespace GensouSakuya.QQBot.Core.Base
{
    internal class ChainOrderAttribute:Attribute
    {
        public int Order { get; private set; }

        public ChainOrderAttribute(int order)
        {
            Order = order;
        }
    }
}
