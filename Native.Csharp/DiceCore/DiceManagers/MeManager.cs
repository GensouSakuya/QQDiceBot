﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace net.gensousakuya.dice
{
    public class MeManager : BaseManager
    {
        public override void Execute(List<string> command, EventSourceType sourceType, UserInfo qq, Group group, GroupMember member)
        {
            var fromQQ = 0L;
            var toGroup = 0L;
            var message = "";
            if (sourceType == EventSourceType.Private)
            {
                fromQQ = qq.QQ;
                if (command.Count < 2)
                {
                    MessageManager.Send(EventSourceType.Private, "指令错误", fromQQ);
                    return;
                }
                
                if (!long.TryParse(command[0], out toGroup))
                {
                    MessageManager.Send(EventSourceType.Private, "发送群号非法", fromQQ);
                    return;
                }

                var mem = GroupMemberManager.Get(fromQQ, toGroup);
                message = mem.GroupName + string.Join(" ", command.Skip(1));

                MessageManager.Send(EventSourceType.Group, message, fromQQ, toGroup);
            }
            else if (sourceType == EventSourceType.Group)
            {
                fromQQ = member.QQ;
                toGroup = member.GroupNumber;
                if (!command.Any())
                {
                    MessageManager.Send(EventSourceType.Group, "指令错误", fromQQ, toGroup);
                    return;
                }

                message = member.GroupName + string.Join(" ", command);

                MessageManager.Send(EventSourceType.Group, message, fromQQ, toGroup);
            }
        }
    }
}