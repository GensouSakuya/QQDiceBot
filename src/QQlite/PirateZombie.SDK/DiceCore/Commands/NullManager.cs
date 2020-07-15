using System.Collections.Generic;
using System.Threading.Tasks;

namespace net.gensousakuya.dice
{
    [Command("null")]
    public class NullManager : BaseManager
    {
        public override async Task ExecuteAsync(List<string> command, EventSourceType sourceType, UserInfo qq, Group group, GroupMember member)
        {
            MessageManager.Send(sourceType, "略略略[QQ:emoji=4036991133]", qq: qq?.QQ, toGroupNo: member?.GroupNumber);
            return;
        }
    }
}
