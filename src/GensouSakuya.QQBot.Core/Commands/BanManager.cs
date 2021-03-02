using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GensouSakuya.QQBot.Core.Base;
using GensouSakuya.QQBot.Core.Model;
using GensouSakuya.QQBot.Core.PlatformModel;
using net.gensousakuya.dice;
using Group = GensouSakuya.QQBot.Core.Model.Group;

namespace GensouSakuya.QQBot.Core.Commands
{
    [Command("ban")]
    public class BanManager : BaseManager
    {
        private static Random _rand = new Random();
        public override async Task ExecuteAsync(List<string> command, List<BaseMessage> originMessage, MessageSourceType sourceType, UserInfo qq, Group group, GroupMember member)
        {
            await Task.Yield();
            var fromQQ = 0L;
            var toGroup = 0L;
            //var message = "";
            if (sourceType != MessageSourceType.Group)
            {
                return;
            }

            fromQQ = member.QQ;
            toGroup = member.GroupNumber;
            var permit = member.PermitType;
            if (permit == PermitType.None)
            {
                MessageManager.SendTextMessage(MessageSourceType.Group, "只有群主或管理员才有权限封禁用户", fromQQ, toGroup);
                return;
            }

            if (!command.Any())
                return;

            if (!long.TryParse(command[0],out var banQQ))
            {
                return;
            }

            if (command.Count > 1 && long.TryParse(command[1], out var banGroup))
            {
                if (DataManager.Instance.GroupBan.ContainsKey((banGroup,banQQ)))
                {
                    DataManager.Instance.GroupBan.TryRemove((banGroup, banQQ), out _);
                    MessageManager.SendTextMessage(MessageSourceType.Group, $"用户{banQQ}在群{banGroup}的封禁已被解除", fromQQ, toGroup);
                    return;
                }
                else
                {
                    DataManager.Instance.GroupBan.TryAdd((banGroup, banQQ), null);
                    MessageManager.SendTextMessage(MessageSourceType.Group, $"用户{banQQ}在群{banGroup}已被封禁", fromQQ, toGroup);
                    return;
                }
            }
            else
            {
                if (DataManager.Instance.QQBan.ContainsKey(banQQ))
                {
                    DataManager.Instance.QQBan.TryRemove(banQQ, out _);
                    MessageManager.SendTextMessage(MessageSourceType.Group, $"用户{banQQ}的全局封禁已被解除", fromQQ, toGroup);
                    return;
                }
                else
                {
                    DataManager.Instance.QQBan.TryAdd(banQQ, null);
                    MessageManager.SendTextMessage(MessageSourceType.Group, $"用户{banQQ}已被全局封禁", fromQQ, toGroup);
                    return;
                }
            }
        }
    }
}
