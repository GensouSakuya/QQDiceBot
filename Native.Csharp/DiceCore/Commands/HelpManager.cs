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
            else
            {
                return;
            }

            var padcount = 0;
            descDic.ToList().ForEach(p =>
            {
                var originLength = p.Key.Length;
                var globalLength = new System.Globalization.StringInfo(p.Key).LengthInTextElements;
                var length = (originLength - globalLength) * 2 + globalLength;
                if (length > padcount)
                {
                    padcount = length;
                }
            });
            StringBuilder desc =
                new StringBuilder().AppendLine(string.Join("\n", descDic.Select(p => $"{p.Key.PadRight(padcount+5)}{p.Value}")));
            if (DataManager.Instance.AdminQQ > 0)
            {
                desc.AppendLine($"bug反馈请联系QQ:{DataManager.Instance.AdminQQ}");
            }
            MessageManager.Send(sourceType, desc.ToString(), qq: qq?.QQ, toGroupNo: member?.GroupNumber);
        }
    }
}
