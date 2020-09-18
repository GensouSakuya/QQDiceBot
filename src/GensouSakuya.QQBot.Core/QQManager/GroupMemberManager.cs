using System.Collections.Generic;
using System.Linq;
using GensouSakuya.QQBot.Core.Model;

namespace GensouSakuya.QQBot.Core.QQManager
{
    public class GroupMemberManager
    {
        public static List<GroupMember> GroupMembers = new List<GroupMember>();

        public static GroupMember Get(long qq,long groupNo)
        {
            var member = GroupMembers.Find(p => p.QQ == qq && p.GroupId == groupNo);
            if (member != null)
                return member;

            var members = PlatformManager.Info.GetGroupMembers(groupNo);
            members.ForEach(p =>
            {
                if (GroupMembers.Any(q => q.QQ == p.QQId && q.GroupId == p.GroupId))
                {
                    //todo update
                }
                else
                {
                    GroupMembers.Add(member);
                }
            });
            member = GroupMembers.Find(p => p.QQ == qq && p.GroupId == groupNo);
            return member;
        }
    }
}
