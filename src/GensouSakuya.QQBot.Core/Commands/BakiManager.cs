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
    [Command("baki")]
    public class BakiManager : BaseManager
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
            if (!command.Any())
            {
                return;
            }

            if (command[0].Equals("on", StringComparison.CurrentCultureIgnoreCase))
            {
                if (permit == PermitType.None)
                {
                    MessageManager.SendTextMessage(MessageSourceType.Group, "只有群主或管理员才有权限开启热狗图功能", fromQQ, toGroup);
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

                DataManager.Instance.GroupBakiConfig.AddOrUpdate(toGroup, config, (p, q) => config);
                MessageManager.SendTextMessage(MessageSourceType.Group, $"随机热狗图已开启，提升纯度概率：{config.Percent}%", fromQQ, toGroup);
            }
            else if (command[0].Equals("off", StringComparison.CurrentCultureIgnoreCase))
            {
                if (permit == PermitType.None)
                {
                    MessageManager.SendTextMessage(MessageSourceType.Group, "只有群主或管理员才有权限关闭热狗图功能", fromQQ, toGroup);
                    return;
                }

                DataManager.Instance.GroupBakiConfig.TryRemove(toGroup, out _);
                MessageManager.SendTextMessage(MessageSourceType.Group, "随机热狗图已关闭", fromQQ, toGroup);
            }
            else if (command[0].Equals("baki", StringComparison.CurrentCultureIgnoreCase))
            {
                var dir = Path.Combine(Config.DataPath, "Baki");
                if (!Directory.Exists(dir))
                    return;
                var files = Directory.GetFiles(dir);
                if (!files.Any())
                    return;
                var fileName = files[_rand.Next(0, files.Length)];
                MessageManager.SendImageMessage(sourceType, fileName, fromQQ, toGroup);
            }
        }
    }

    public class BakiConfig
    {
        public int Percent { get; set; }
    }
}
