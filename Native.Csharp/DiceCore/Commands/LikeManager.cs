using Native.Csharp.App;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace net.gensousakuya.dice
{
    public class LikeManager : BaseManager
    {
        public override async Task ExecuteAsync(List<string> command, EventSourceType sourceType, UserInfo qq, Group group, GroupMember member)
        {
            if (sourceType != EventSourceType.Private)
            {
                return;
            }

            Common.CqApi.SendPraise(qq.QQ);
            MessageManager.Send(sourceType, "赞了赞了！", qq?.QQ);
        }
    }
}
