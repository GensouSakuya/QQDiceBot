using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace net.gensousakuya.dice
{
    public class AdminManager : BaseManager
    {
        public override void Execute(List<string> command, EventSourceType sourceType, UserInfo qq, Group group, GroupMember member)
        {
            if (sourceType != EventSourceType.Private)
                return;
            if (qq == null || qq.QQ != DataManager.Instance.AdminQQ)
                return;
            var firstCommand = command.FirstOrDefault();
            if (firstCommand == null)
                return;
            switch (firstCommand.ToLower())
            {
                case "say":
                    {
                        command.RemoveAt(0);
                        if (command.Count < 2)
                            return;
                        var groupNumber = 0L;
                        if (!long.TryParse(command.First(), out groupNumber))
                        {
                            return;
                        }

                        command.RemoveAt(0);
                        var message = string.Join(" ", command);
                        MessageManager.Send(EventSourceType.Group, message, qq?.QQ, groupNumber);
                        return;
                    }
            }
        }
    }
}
