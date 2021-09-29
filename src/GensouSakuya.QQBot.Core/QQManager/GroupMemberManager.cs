using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GensouSakuya.QQBot.Core.Model;

namespace GensouSakuya.QQBot.Core.QQManager
{
    public class GroupMemberManager
    {
        public static ConcurrentDictionary<(long,long),GroupMember> GroupMembers = new ConcurrentDictionary<(long,long),GroupMember>();

        public static async Task<GroupMember> Get(long qq, long groupNo)
        {
            if (GroupMembers.TryGetValue((qq,groupNo),out var member))
                return member;

            var sourceMembers = await PlatformManager.Info.GetGroupMembers(groupNo);
            if (sourceMembers == null)
                return null;

            sourceMembers.ForEach(p =>
            {
                GroupMembers.AddOrUpdate((qq, groupNo), new GroupMember(p), (ids, p) => p);
            });

            return GroupMembers.TryGetValue((qq, groupNo), out member) ? member : null;
        }

        public static Task StartLoadTask(CancellationToken token)
        {
            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    var groupIds = GroupMembers.Values.Select(p => p.GroupNumber).Distinct();
                    foreach(var groupId in groupIds)
                    {
                        var sourceMembers = await PlatformManager.Info.GetGroupMembers(groupId);
                        if (sourceMembers != null)
                        {
                            sourceMembers.ForEach(p =>
                            {
                                if (GroupMembers.TryGetValue((p.QQId,p.GroupId),out var tmember))
                                {
                                    tmember.Card = p.Card;
                                    tmember.PermitType = p.PermitType;
                                }
                                else
                                {
                                    GroupMembers.TryAdd((p.QQId,p.GroupId), new GroupMember(p));
                                }
                            });
                        }
                    }
                    await Task.Delay(TimeSpan.FromMinutes(5));
                }
            });
            return Task.CompletedTask;
        }
    }
}
