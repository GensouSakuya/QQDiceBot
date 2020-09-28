using System.Collections.Generic;
using System.Threading.Tasks;
using GensouSakuya.QQBot.Core.Base;
using GensouSakuya.QQBot.Core.Model;
using GensouSakuya.QQBot.Core.PlatformModel;
using net.gensousakuya.dice;

namespace GensouSakuya.QQBot.Core.Commands
{
    [Command("null")]
    public class NullManager : BaseManager
    {
        public override async Task ExecuteAsync(List<string> command, List<BaseMessage> originMessage, MessageSourceType sourceType, UserInfo qq, Group group, GroupMember member)
        {
            await Task.Yield();
            MessageManager.SendTextMessage(sourceType, "略略略😝", qq: qq?.QQ, toGroupNo: member?.GroupNumber);
            return;
        }
    }
}
