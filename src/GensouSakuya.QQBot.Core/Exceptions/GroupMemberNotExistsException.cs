using System;

namespace GensouSakuya.QQBot.Core.Exceptions
{
    public class GroupMemberNotExistsException : Exception
    {
        public long QQ { get; private set; }
        public long GroupNo { get; private set; }
        public GroupMemberNotExistsException(long groupNo, long qq)
        {
            GroupNo = groupNo;
            QQ = qq;
        }
    }
}
