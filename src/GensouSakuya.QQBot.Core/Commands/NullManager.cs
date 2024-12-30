using System.Collections.Generic;
using System.Threading.Tasks;
using GensouSakuya.QQBot.Core.Base;
using GensouSakuya.QQBot.Core.Model;
using GensouSakuya.QQBot.Core.PlatformModel;
using net.gensousakuya.dice;

namespace GensouSakuya.QQBot.Core.Commands
{
    [Command("null")]
    public class NullHandler : BaseManager
    {
        public override async Task ExecuteAsync(MessageSource source, List<string> command, List<BaseMessage> originMessage, UserInfo qq, Group group, GroupMember member, GuildUserInfo guildUser, GuildMember guildmember)
        {
            await Task.Yield();
            MessageManager.SendToSource(source, "略略略😝");
            return;
        }
    }
}
