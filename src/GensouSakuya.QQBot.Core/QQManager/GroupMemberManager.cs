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
        public static async Task<GroupMember> Get(long qq,long groupNo)
        {
            var member = GroupMembers.Find(p => p.QQ == qq && p.GroupId == groupNo);

            var sourceMember = await PlatformManager.Info.GetGroupoMember(groupNo, qq);

            if (member != null)
            {
                member.Card = sourceMember.Card;
            }
            else
            {
                member = new GroupMember(sourceMember);
                lock (AddLock)
                {
                    if (GroupMembers.All(p => p.QQ != qq && p.GroupId != groupNo))
                    {
                        GroupMembers.Add(member);
                    }
                }
            }
            return member;
        }
    }
}
