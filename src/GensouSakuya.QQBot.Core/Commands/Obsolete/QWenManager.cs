//using GensouSakuya.QQBot.Core.Base;
//using GensouSakuya.QQBot.Core.Model;
//using GensouSakuya.QQBot.Core.PlatformModel;
//using net.gensousakuya.dice;
//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net.Http;
//using System.Text;

//namespace GensouSakuya.QQBot.Core.Commands
//{
//    [Command("ai")]
//    public class QWenManager : BaseManager
//    {
//        private static readonly Logger _logger = Logger.GetLogger<QWenManager>();

//        public override async System.Threading.Tasks.Task ExecuteAsync(MessageSource source, List<string> command, List<BaseMessage> originMessage, UserInfo qq, Group group, GroupMember member, GuildUserInfo guildUser, GuildMember guildmember)
//        {
//            var fromQQ = 0L;
//            var toGroup = 0L;
//            //var message = "";
//            if (source.Type != MessageSourceType.Group)
//            {
//                return;
//            }

//            fromQQ = member.QQ;
//            toGroup = member.GroupNumber;
//            var config = DataManager.Instance?.GroupQWenConfig;
//            if (config == null)
//                return;

//            if (!command.Any())
//            {
//                if (!config.TryGetValue(toGroup, out _))
//                {
//                    MessageManager.SendTextMessage(MessageSourceType.Group, "当前群尚未开启AI对话功能", fromQQ, toGroup);
//                    return;
//                }
//            }
//            else
//            {
//                if (command[0].Equals("on", StringComparison.CurrentCultureIgnoreCase))
//                {
//                    if (!member.IsGroupAdmin() && !Tools.IsRobotAdmin(fromQQ))
//                    {
//                        MessageManager.SendTextMessage(MessageSourceType.Group, "只有群主或管理员才有权限开启AI对话功能", fromQQ, toGroup);
//                        return;
//                    }

//                    UpdateGroupQWenConfig(toGroup, true);
//                    MessageManager.SendTextMessage(MessageSourceType.Group, "AI对话功能已开启", fromQQ, toGroup);
//                    return;
//                }
//                else if (command[0].Equals("off", StringComparison.CurrentCultureIgnoreCase))
//                {
//                    if (!member.IsGroupAdmin() && !Tools.IsRobotAdmin(fromQQ))
//                    {
//                        MessageManager.SendTextMessage(MessageSourceType.Group, "只有群主或管理员才有权限关闭AI对话功能", fromQQ, toGroup);
//                        return;
//                    }

//                    UpdateGroupQWenConfig(toGroup, false);
//                    MessageManager.SendTextMessage(MessageSourceType.Group, "AI对话已关闭", fromQQ, toGroup);
//                    return;
//                }
//            }
//        }

//        public void UpdateGroupQWenConfig(long toGroup, bool enable)
//        {
//            if (enable)
//                DataManager.Instance?.GroupQWenConfig.AddOrUpdate(toGroup, enable, (p, q) => enable);
//            else
//                DataManager.Instance?.GroupQWenConfig.TryRemove(toGroup, out _);
//            DataManager.NoticeConfigUpdatedAction();
//        }
//    }
//}
