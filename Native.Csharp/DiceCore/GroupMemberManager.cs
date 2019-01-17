using Native.Csharp.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace net.gensousakuya.dice
{
    public class GroupMemberManager
    {
        private static List<GroupMember> _groupMembers
        {
            get { return DataManager.Instance.GroupMember; }
        }

        public static GroupMember Get(long qq,long groupNo)
        {
            var gm = _groupMembers.Find(p => p.QQ == qq && p.GroupNumber == groupNo);
            if (gm == null)
            {
                Common.CqApi.GetMemberInfo(groupNo, qq, out Native.Csharp.Sdk.Cqp.Model.GroupMember groupMember);
                gm = new GroupMember(groupMember);
            }

            return gm;
        }
    }
}
