//using GensouSakuya.QQBot.Core.Base;
//using GensouSakuya.QQBot.Core.Interfaces;
//using GensouSakuya.QQBot.Core.Model;
//using GensouSakuya.QQBot.Core.PlatformModel;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace GensouSakuya.QQBot.Core.Handlers
//{
//    internal class AiHandler: IMessageCommandHandler
//    {
//        private DataManager _dataManager;

//        public AiHandler(DataManager dataManager)
//        {
//            _dataManager = dataManager;
//        }

//        public async Task<bool> ExecuteAsync(MessageSource source, IEnumerable<string> command, List<BaseMessage> originMessage, SourceFullInfo sourceInfo)
//        {
//            await Task.Yield();
//            var fromQQ = 0L;
//            var toGroup = 0L;
//            //var message = "";
//            if (source.Type != MessageSourceType.Group && source.Type != MessageSourceType.Private)
//            {
//                return false;
//            }

//            fromQQ = sourceInfo.GroupMember.QQ;
//            toGroup = sourceInfo.GroupMember.GroupNumber;
//            var config = _dataManager.Config?.AiEnableConifig;
//            if (config == null)
//                return false;

//            if (!command.Any())
//            {
//                if (!config.TryGetValue(toGroup, out _))
//                {
//                    MessageManager.SendTextMessage(MessageSourceType.Group, "当前群尚未开启AI对话功能", fromQQ, toGroup);
//                    return false;
//                }
//            }
//            else
//            {
//                if (command.ElementAt(0).Equals("on", StringComparison.CurrentCultureIgnoreCase))
//                {
//                    if (!Tools.IsRobotAdmin(fromQQ))
//                    {
//                        MessageManager.SendTextMessage(MessageSourceType.Group, "只有群主或管理员才有权限开启AI对话功能", fromQQ, toGroup);
//                        return false;
//                    }

//                    UpdateGroupQWenConfig(toGroup, true);
//                    MessageManager.SendTextMessage(MessageSourceType.Group, "AI对话功能已开启", fromQQ, toGroup);
//                }
//                else if (command.ElementAt(0).Equals("off", StringComparison.CurrentCultureIgnoreCase))
//                {
//                    if (!Tools.IsRobotAdmin(fromQQ))
//                    {
//                        MessageManager.SendTextMessage(MessageSourceType.Group, "只有群主或管理员才有权限关闭AI对话功能", fromQQ, toGroup);
//                        return false;
//                    }

//                    UpdateGroupQWenConfig(toGroup, false);
//                    MessageManager.SendTextMessage(MessageSourceType.Group, "AI对话已关闭", fromQQ, toGroup);
//                }
//            }
//            return true;
//        }

//        public void UpdateGroupQWenConfig(long toGroup, bool enable)
//        {
//            if (enable)
//                _dataManager?.Config.AiEnableConifig.AddOrUpdate(toGroup, enable, (p, q) => enable);
//            else
//                _dataManager?.Config.AiEnableConifig.TryRemove(toGroup, out _);
//            _dataManager.NoticeConfigUpdated();
//        }
//    }
//}
