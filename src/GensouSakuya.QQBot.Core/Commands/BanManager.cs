using System;
using System.Collections.Concurrent;
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
        public override async Task ExecuteAsync(MessageSource source, List<string> command, List<BaseMessage> originMessage, UserInfo qq, Group group, GroupMember member, GuildUserInfo guildUser, GuildMember guildmember)
        {
            await Task.Yield();
            var fromQQ = 0L;
            var toGroup = 0L;
            //var message = "";
            if (source.Type != MessageSourceType.Group)
            {
                return;
            }

            fromQQ = member.QQ;
            toGroup = member.GroupNumber;
            var permit = member.PermitType;
            if (permit == PermitType.None)
            {
                MessageManager.SendToSource(source, "只有群主或管理员才有权限封禁用户");
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
                if (GroupBan.ContainsKey((banGroup,banQQ)))
                {
                    UpdateGroupQQBan(banGroup, banQQ, false);
                    MessageManager.SendToSource(source, $"用户{banQQ}在群{banGroup}的封禁已被解除");
                    return;
                }
                else
                {
                    UpdateGroupQQBan(banGroup, banQQ, true);
                    MessageManager.SendToSource(source, $"用户{banQQ}在群{banGroup}已被封禁");
                    return;
                }
            }
            else
            {
                if (QQBan.ContainsKey(banQQ))
                {
                    UpdateQQBan(banQQ, false);
                    MessageManager.SendToSource(source, $"用户{banQQ}的全局封禁已被解除");
                    return;
                }
                else
                {
                    UpdateQQBan(banQQ, true);
                    MessageManager.SendToSource(source, $"用户{banQQ}已被全局封禁");
                    return;
                }
            }
        }

        
        private static ConcurrentDictionary<long,string> _qqBan = new ConcurrentDictionary<long, string>();
        public static ConcurrentDictionary<long, string> QQBan
        {
            get => _qqBan;
            set
            {
                if (value == null)
                {
                    _qqBan = new ConcurrentDictionary<long, string>();
                }
                else
                {
                    _qqBan = value;
                }
            }
        }

        public void UpdateQQBan(long banQQ, bool isBan)
        {
            if(isBan)
                QQBan.TryAdd(banQQ, null);
            else
                QQBan.TryRemove(banQQ, out _);
            DataManager.NoticeConfigUpdatedAction();
        }

        private static ConcurrentDictionary<(long,long), string> _groupBan = new ConcurrentDictionary<(long, long), string>();
        public static ConcurrentDictionary<(long, long), string> GroupBan
        {
            get => _groupBan;
            set
            {
                if (value == null)
                {
                    _groupBan = new ConcurrentDictionary<(long, long), string>();
                }
                else
                {
                    _groupBan = value;
                }
            }
        }

        public void UpdateGroupQQBan(long banGroup, long banQQ, bool isBan)
        {
            if(isBan)
                GroupBan.TryAdd((banGroup,banQQ), null);
            else
                GroupBan.TryRemove((banGroup,banQQ), out _);
            DataManager.NoticeConfigUpdatedAction();
        }
    }
}
