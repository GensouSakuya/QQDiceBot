using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace net.gensousakuya.dice
{
    public class HelpManager: BaseManager
    {
        public override void Execute(List<string> command, EventSourceType sourceType, UserInfo qq, Group group, GroupMember member)
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

            StringBuilder desc =
                new StringBuilder().AppendLine(string.Join("\n", descDic.Select(p => $"{p.Key.PadLeft(4).PadRight(40)}{p.Value}")));
            desc.AppendLine($"bug反馈请联系QQ:{DataManager.Instance.AdminQQ}");
            MessageManager.Send(sourceType, desc.ToString(), qq: qq?.QQ, toGroupNo: member?.GroupNumber);
        }
    }
}
