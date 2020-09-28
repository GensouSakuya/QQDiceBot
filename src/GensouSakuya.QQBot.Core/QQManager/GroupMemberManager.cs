using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GensouSakuya.QQBot.Core.Model;

namespace GensouSakuya.QQBot.Core.QQManager
{
    public class GroupMemberManager
    {
        public static List<GroupMember> GroupMembers = new List<GroupMember>();

        private static readonly object AddLock = new object();

        public static async Task<GroupMember> Get(long qq, long groupNo)
        {
            var member = GroupMembers.Find(p => p.QQ == qq && p.GroupId == groupNo);
            if (member != null)
                return member;

            var sourceMembers = await PlatformManager.Info.GetGroupMembers(groupNo);
            if (sourceMembers == null)
                return member;

            sourceMembers.ForEach(p =>
            {
                var tmember = GroupMembers.Find(p => p.QQ == qq && p.GroupId == groupNo);
                if (tmember == null)
                {
                    GroupMembers.Add(new GroupMember(tmember));
                }
                else
                {
                    tmember.Card = p.Card;
                    tmember.PermitType = p.PermitType;
                }
            });

            member = GroupMembers.Find(p => p.QQ == qq && p.GroupId == groupNo);
            return member;
        }
    }
}
