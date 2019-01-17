using System.Collections.Generic;

namespace net.gensousakuya.dice
{
    public abstract class BaseManager
    {
        public abstract void Execute(List<string> command, EventSourceType sourceType, UserInfo qq, Group group, GroupMember member);
    }
}
