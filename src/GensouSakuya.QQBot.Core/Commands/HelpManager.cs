using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GensouSakuya.QQBot.Core.Base;
using GensouSakuya.QQBot.Core.Model;
using GensouSakuya.QQBot.Core.PlatformModel;
using net.gensousakuya.dice;

namespace GensouSakuya.QQBot.Core.Commands
{
    [Command("help")]
    public class HelpManager: BaseManager
    {
        public override async Task ExecuteAsync(MessageSource source, List<string> command, List<BaseMessage> originMessage, UserInfo qq, Group group, GroupMember member, GuildUserInfo guildUser, GuildMember guildmember)
        {
            await Task.Yield();
            Dictionary<string, string> descDic = null;
            if (source.Type == MessageSourceType.Group || source.Type == MessageSourceType.Guild)
            {
                descDic = Config.GroupCommandDesc;
            }
            else if (source.Type == MessageSourceType.Private)
            {
                descDic = Config.PrivateCommandDesc;
            }
            else
            {
                return;
            }

            StringBuilder desc =
                new StringBuilder().AppendLine(string.Join("\n", descDic.Select(p => $"{p.Key}\t\t{p.Value}")));
            //if (DataManager.Instance.AdminQQ > 0)
            //{
            //    desc.Append($"bug反馈请联系QQ:{DataManager.Instance.AdminQQ}");
            //}
            MessageManager.SendToSource(source, desc.ToString());
        }
    }
}
