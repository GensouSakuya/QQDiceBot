using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GensouSakuya.QQBot.Core.Base;
using GensouSakuya.QQBot.Core.Model;
using GensouSakuya.QQBot.Core.PlatformModel;
using GensouSakuya.QQBot.Core.QQManager;
using net.gensousakuya.dice;

namespace GensouSakuya.QQBot.Core.Commands
{
    [Command("me")]
    public class MeManager : BaseManager
    {
        public override async Task ExecuteAsync(List<string> command, MessageSourceType sourceType, UserInfo qq, Group group, GroupMember member)
        {
            var fromQQ = 0L;
            var toGroup = 0L;
            var message = "";
            if (sourceType == MessageSourceType.Private)
            {
                fromQQ = qq.QQ;
                if (command.Count < 2)
                {
                    MessageManager.Send(MessageSourceType.Private, "你不说话我怎么知道你想让我帮你说什么0 0", fromQQ);
                    return;
                }
                
                if (!long.TryParse(command[0], out toGroup))
                {
                    MessageManager.Send(MessageSourceType.Private, "小夜看不明白你想把这段话发到哪", fromQQ);
                    return;
                }

                var mem = GroupMemberManager.Get(fromQQ, toGroup);
                message = mem.GroupName + string.Join(" ", command.Skip(1));

                MessageManager.Send(MessageSourceType.Group, message, fromQQ, toGroup);
            }
            else if (sourceType == MessageSourceType.Group)
            {
                fromQQ = member.QQ;
                toGroup = member.GroupNumber;
                if (!command.Any())
                {
                    MessageManager.Send(MessageSourceType.Group, "你不说话我怎么知道你想让我帮你说什么0 0", fromQQ, toGroup);
                    return;
                }

                message = member.GroupName + string.Join(" ", command);

                MessageManager.Send(MessageSourceType.Group, message, fromQQ, toGroup);
            }
        }
    }
}
