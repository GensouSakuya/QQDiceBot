using System.Collections.Generic;
using System.Threading.Tasks;

namespace net.gensousakuya.dice
{
    public abstract class BaseManager
    {
        public abstract async Task ExecuteAsync(List<string> command, EventSourceType sourceType, UserInfo qq, Group group, GroupMember member);
    }
}
