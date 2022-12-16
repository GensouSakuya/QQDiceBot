using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GensouSakuya.QQBot.Core.Base;
using GensouSakuya.QQBot.Core.Model;
using GensouSakuya.QQBot.Core.PlatformModel;
using net.gensousakuya.dice;
using Group = GensouSakuya.QQBot.Core.Model.Group;

namespace GensouSakuya.QQBot.Core.Commands
{
    [Command("fudu")]
    public class RepeatManager : BaseManager
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
            if (!command.Any())
            {
                if (!GroupRepeatConfig.TryGetValue(toGroup, out var config))
                {
                    MessageManager.SendToSource(source, "当前群尚未开启复读功能");
                }
                else
                {
                    MessageManager.SendToSource(source, $"当前复读概率：{config.Percent}%");
                }

                return;
            }

            if (command[0].Equals("on", StringComparison.CurrentCultureIgnoreCase))
            {
                if (permit == PermitType.None)
                {
                    MessageManager.SendToSource(source, "只有群主或管理员才有权限开启复读功能");
                    return;
                }
                RepeatConfig config;

                if (command.Count == 1)
                {
                    config = new RepeatConfig();
                }
                else
                {
                    if (int.TryParse(command[1], out var percent))
                    {
                        config = new RepeatConfig
                        {
                            Percent = percent > 100 ? 100 : percent
                        };
                    }
                    else
                    {
                        config = new RepeatConfig();
                    }
                }

                UpdateGroupRepeatConfig(toGroup, config);

                MessageManager.SendToSource(source, $"复读已开启，复读概率：{config.Percent}%");
            }
            else if (command[0].Equals("off", StringComparison.CurrentCultureIgnoreCase))
            {
                if (permit == PermitType.None)
                {
                    MessageManager.SendToSource(source, "只有群主或管理员才有权限关闭复读功能");
                    return;
                }

                UpdateGroupRepeatConfig(toGroup, null);
                MessageManager.SendToSource(source, "复读已关闭");
            }
            else if (command[0].Equals("repeat", StringComparison.CurrentCultureIgnoreCase) && command.Count>1)
            {
                MessageManager.SendToSource(source, originMessage);
            }
        }

        private static ConcurrentDictionary<long, RepeatConfig> _groupRepeatConfig = new ConcurrentDictionary<long, RepeatConfig>();
        public static ConcurrentDictionary<long, RepeatConfig> GroupRepeatConfig
        {
            get => _groupRepeatConfig;
            set
            {
                if (value == null)
                {
                    _groupRepeatConfig = new ConcurrentDictionary<long, RepeatConfig>();
                }
                else
                {
                    _groupRepeatConfig = value;
                }
            }
        }

        public static void UpdateGroupRepeatConfig(long toGroup, RepeatConfig config)
        {
            if(config != null)
                GroupRepeatConfig.AddOrUpdate(toGroup, config, (p, q) => config);
            else
                GroupRepeatConfig.TryRemove(toGroup, out _);
            DataManager.Instance.NoticeConfigUpdated();
        }
    }

    public class RepeatConfig
    {
        public int Percent { get; set; } = 5;
    }
}
