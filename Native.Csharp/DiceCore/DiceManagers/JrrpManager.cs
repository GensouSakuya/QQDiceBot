using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace net.gensousakuya.dice
{
    public class JrrpManager: BaseManager
    {
        private static List<long> _disabledJrrpGroupNumbers
        {
            get { return DataManager.Instance.DisabledJrrpGroupNumbers; }
        }
        public static int GetJrrp(UserInfo user)
        {
            if (!user.Jrrp.HasValue)
            {
                user.Jrrp = DiceManager.RollDice();
            }

            return user.Jrrp.Value;
        }

        public override void Execute(List<string> command, EventSourceType sourceType, UserInfo qq, Group group, GroupMember member)
        {
            var name = "";
            if (sourceType == EventSourceType.Group)
            {
                if (member == null)
                    return;
                if (_disabledJrrpGroupNumbers.Contains(member.GroupNumber))
                {
                    return;
                }

                name = string.IsNullOrWhiteSpace(member.GroupName) ? qq.Name : member.GroupName;
            }

            var rp = GetJrrp(qq);
            MessageManager.Send(sourceType, name + "今天的人品值是:" + rp, qq: qq?.QQ, toGroupNo: member?.GroupNumber);
        }
    }
}
