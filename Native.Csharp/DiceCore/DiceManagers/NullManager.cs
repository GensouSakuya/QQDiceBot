using System.Collections.Generic;

namespace net.gensousakuya.dice
{
    public class NullManager : BaseManager
    {
        public override void Execute(List<string> command, EventSourceType sourceType, UserInfo qq, Group group, GroupMember member)
        {
            MessageManager.Send(sourceType, "略略略😝", qq: qq?.QQ, toGroupNo: member?.GroupNumber);
            return;
        }
    }
}
