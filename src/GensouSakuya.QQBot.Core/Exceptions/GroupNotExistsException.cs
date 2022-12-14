using System;

namespace GensouSakuya.QQBot.Core.Exceptions
{
    public class GroupNotExistsException:Exception
    {
        public long GroupNo { get; private set; }
        public GroupNotExistsException(long groupNo)
        {
            GroupNo = groupNo;
        }
    }
}
