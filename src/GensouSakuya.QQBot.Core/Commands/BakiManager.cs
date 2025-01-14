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
    [Command("baki")]
    public class BakiManager : BaseManager
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
            if (!command.Any())
            {
                if (!GroupBakiConfig.TryGetValue(toGroup, out var config))
                {
                    MessageManager.SendToSource(source, "当前群尚未开启热狗图功能");
                }
                else
                {
                    MessageManager.SendToSource(source, $"当前热狗人纯度：{config.Percent}%");
                }
                
                return;
            }

            if (command[0].Equals("on", StringComparison.CurrentCultureIgnoreCase))
            {
                if (!member.IsGroupAdmin() && !Tools.IsRobotAdmin(fromQQ))
                {
                    MessageManager.SendToSource(source, "只有群主或管理员才有权限开启热狗图功能");
                    return;
                }
                BakiConfig config;

                if (command.Count == 1)
                {
                    config = new BakiConfig();
                }
                else
                {
                    if (int.TryParse(command[1], out var percent))
                    {
                        config = new BakiConfig
                        {
                            Percent = percent > 100 ? 100 : percent
                        };
                    }
                    else
                    {
                        config = new BakiConfig();
                    }
                }

                UpdateGroupBakiConfig(toGroup, config);
                MessageManager.SendToSource(source, $"随机热狗图已开启，提升纯度概率：{config.Percent}%");
            }
            else if (command[0].Equals("off", StringComparison.CurrentCultureIgnoreCase))
            {
                if (!member.IsGroupAdmin() && !Tools.IsRobotAdmin(fromQQ))
                {
                    MessageManager.SendToSource(source, "只有群主或管理员才有权限关闭热狗图功能");
                    return;
                }
                
                UpdateGroupBakiConfig(toGroup, null);
                MessageManager.SendToSource(source, "随机热狗图已关闭");
            }
            else if (command[0].Equals("baki", StringComparison.CurrentCultureIgnoreCase))
            {
                var dir = Path.Combine(DataManager.DataPath, "Baki");
                if (!Directory.Exists(dir))
                    return;
                var files = Directory.GetFiles(dir);
                if (!files.Any())
                    return;
                var fileName = files[_rand.Next(0, files.Length)];
                MessageManager.SendImageMessage(source.Type, fileName, fromQQ, toGroup);
            }
        }
        
        private static ConcurrentDictionary<long, BakiConfig> _groupBakiConfig = new ConcurrentDictionary<long, BakiConfig>();
        public static ConcurrentDictionary<long, BakiConfig> GroupBakiConfig
        {
            get => _groupBakiConfig;
            set
            {
                if (value == null)
                {
                    _groupBakiConfig = new ConcurrentDictionary<long, BakiConfig>();
                }
                else
                {
                    _groupBakiConfig = value;
                }
            }
        }

        public void UpdateGroupBakiConfig(long toGroup, BakiConfig config)
        {
            if(config != null)
                GroupBakiConfig.AddOrUpdate(toGroup, config, (p, q) => config);
            else
                GroupBakiConfig.TryRemove(toGroup, out _);
            DataManager.NoticeConfigUpdatedAction();
        }
    }

    public class BakiConfig
    {
        public int Percent { get; set; }
    }
}
