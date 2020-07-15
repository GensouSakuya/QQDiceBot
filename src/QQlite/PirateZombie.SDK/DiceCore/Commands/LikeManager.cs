using PirateZombie.SDK;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace net.gensousakuya.dice
{
    [Command("like")]
    public class LikeManager : BaseManager
    {
        public override async Task ExecuteAsync(List<string> command, EventSourceType sourceType, UserInfo qq, Group group, GroupMember member)
        {
            if (sourceType != EventSourceType.Private)
            {
                return;
            }

            QLAPI.Api_SendPraise(qq.QQ.ToString(), QLMain.ac);
            MessageManager.Send(sourceType, "赞了赞了！", qq?.QQ);
        }
    }
}
