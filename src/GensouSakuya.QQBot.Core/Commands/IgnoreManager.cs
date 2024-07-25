using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GensouSakuya.QQBot.Core.Base;
using GensouSakuya.QQBot.Core.Model;
using GensouSakuya.QQBot.Core.PlatformModel;
using net.gensousakuya.dice;

namespace GensouSakuya.QQBot.Core.Commands
{
    [Command("ignore")]
    public class IgnoreManager : BaseManager
    {
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
            if (!member.IsGroupAdmin() && !Tools.IsRobotAdmin(fromQQ))
            {
                MessageManager.SendTextMessage(MessageSourceType.Group, "只有群主或管理员才有权限忽略用户", fromQQ, toGroup);
                return;
            }

            if (!command.Any())
                return;

            if (!long.TryParse(command[0], out var banQQ))
            {
                return;
            }

            if (GroupIgnore.ContainsKey((member.GroupId, banQQ)))
            {
                UpdateGroupIgnore(member.GroupId, banQQ, false);
                MessageManager.SendTextMessage(MessageSourceType.Group, $"用户{banQQ}在群{member.GroupId}已解除忽略", fromQQ, toGroup);
                return;
            }
            else
            {
                UpdateGroupIgnore(member.GroupId, banQQ, true);
                MessageManager.SendTextMessage(MessageSourceType.Group, $"用户{banQQ}在群{member.GroupId}已被忽略", fromQQ, toGroup);
                return;
            }
        }


        private static ConcurrentDictionary<(long, long), string> _groupIgnore = new ConcurrentDictionary<(long, long), string>();
        public static ConcurrentDictionary<(long, long), string> GroupIgnore
        {
            get => _groupIgnore;
            set
            {
                if (value == null)
                {
                    _groupIgnore = new ConcurrentDictionary<(long, long), string>();
                }
                else
                {
                    _groupIgnore = value;
                }
            }
        }

        public void UpdateGroupIgnore(long banGroup, long banQQ, bool isBan)
        {
            if (isBan)
                GroupIgnore.TryAdd((banGroup, banQQ), null);
            else
                GroupIgnore.TryRemove((banGroup, banQQ), out _);
            DataManager.Instance.NoticeConfigUpdated();
        }
    }
}
