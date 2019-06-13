using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static net.gensousakuya.dice.Tools;

namespace net.gensousakuya.dice
{
    public static class CommandCenter
    {
        public static void Execute(string command, EventSourceType sourceType,long? qqNo = null, long? groupNo = null)
        {
            if (!command.StartsWith(".") && !command.StartsWith("/"))
                return;

            UserInfo qq = null;
            if(qqNo.HasValue)
            {
                qq = UserManager.Get(qqNo.Value);
            }
            Group group = null;
            //if (groupNo.HasValue)
            //{
            //    group = GroupManager.Get(groupNo.Value);
            //}
            GroupMember member = null;
            if(qqNo.HasValue && groupNo.HasValue)
            {
                member = GroupMemberManager.Get(qqNo.Value, groupNo.Value);
            }

            var commandStr = command.Remove(0, 1);
            var commandList = TakeCommandParts(commandStr);

            BaseManager manager = null;
            var commandName = commandList.FirstOrDefault();
            if (commandName == null)
                return;
            switch (commandName.ToLower())
            {
                case "jrrp":
                    manager = new JrrpManager();
                    break;
                case "nn":
                    manager = new NickNameManager();
                    break;
                case "me":
                    manager = new MeManager();
                    break;
                case "coc7":
                case "coc6d":
                    break;
                case "ti":
                    manager = new TempInsaneManager();
                    break;
                case "li":
                    manager = new LongInsaneManager();
                    break;
                case "ask":
                    manager = new AskManager();
                    break;
                case "admin":
                    manager = new AdminManager();
                    break;
                case "null":
                    manager = new NullManager();
                    break;
                case "like":
                    manager = new LikeManager();
                    break;
                default:
                    return;
            }

            commandList.RemoveAt(0);
            var args = commandList;
            manager.Execute(args, sourceType, qq, group, member);
        }
    }
}
