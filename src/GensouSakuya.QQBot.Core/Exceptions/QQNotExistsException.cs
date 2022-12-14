using System;

namespace GensouSakuya.QQBot.Core.Exceptions
{
    public class QQNotExistsException : Exception
    {
        public long QQ { get; private set; }
        public QQNotExistsException(long qq)
        {
            QQ = qq;
        }
    }
}
