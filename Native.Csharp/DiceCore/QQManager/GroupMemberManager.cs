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
            Common.CqApi.GetMemberInfo(groupNo, qq, out Native.Csharp.Sdk.Cqp.Model.GroupMember groupMember);
            var gm = _groupMembers.Find(p => p.QQ == qq && p.GroupNumber == groupNo);
            if (gm == null)
            {
                gm = new GroupMember(groupMember);
                _groupMembers.Add(gm);
            }
            else
            {
                gm.Copy(groupMember);
            }

            return gm;
        }
    }
}
