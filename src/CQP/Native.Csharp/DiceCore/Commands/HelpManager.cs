﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace net.gensousakuya.dice
{
    [Command("help")]
    public class HelpManager: BaseManager
    {
        public override async Task ExecuteAsync(List<string> command, EventSourceType sourceType, UserInfo qq, Group group, GroupMember member)
        {
            Dictionary<string, string> descDic = null;
            if (sourceType == EventSourceType.Group)
            {
                descDic = Config.GroupCommandDesc;
            }
            else if (sourceType == EventSourceType.Private)
            {
                descDic = Config.PrivateCommandDesc;
            }
            else
            {
                return;
            }

            StringBuilder desc =
                new StringBuilder().AppendLine(string.Join("\n", descDic.Select(p => $"{p.Key}\t\t{p.Value}")));
            if (DataManager.Instance.AdminQQ > 0)
            {
                desc.Append($"bug反馈请联系QQ:{DataManager.Instance.AdminQQ}");
            }
            MessageManager.Send(sourceType, desc.ToString(), qq: qq?.QQ, toGroupNo: member?.GroupNumber);
        }
    }
}
