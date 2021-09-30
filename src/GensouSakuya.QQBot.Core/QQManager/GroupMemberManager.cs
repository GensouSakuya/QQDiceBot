using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GensouSakuya.QQBot.Core.Base;
using GensouSakuya.QQBot.Core.Exceptions;
using GensouSakuya.QQBot.Core.Model;

namespace GensouSakuya.QQBot.Core.QQManager
{
    public class GroupMemberManager
    {
        private static readonly Logger _logger = Logger.GetLogger<GroupMemberManager>();
        public static ConcurrentDictionary<(long qq, long gruopNo), GroupMember> GroupMembers = new ConcurrentDictionary<(long, long), GroupMember>();

        public static async Task<GroupMember> Get(long qq, long groupNo)
        {
            if (GroupMembers.TryGetValue((qq, groupNo), out var member))
                return member;

            var sourceMembers = await PlatformManager.Info.GetGroupMembers(groupNo);
            if (sourceMembers == null)
                return null;

            sourceMembers.ForEach(p =>
            {
                GroupMembers.AddOrUpdate((p.QQId, p.GroupId), new GroupMember(p), (ids, updateMember) =>
                {
                    updateMember.Card = p.Card;
                    updateMember.PermitType = p.PermitType;
                    return updateMember;
                });
            });

            DataManager.Instance.NoticeConfigUpdated();

            return GroupMembers.TryGetValue((qq, groupNo), out member) ? member : null;
        }

        public static Task StartLoadTask(CancellationToken token)
        {
            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    var hasUpdate = false;
                    try
                    {
                        var groupIds = GroupMembers.Values.Select(p => p.GroupNumber).Distinct();
                        foreach (var groupId in groupIds)
                        {
                            try
                            {
                                var sourceMembers = await PlatformManager.Info.GetGroupMembers(groupId);
                                sourceMembers?.ForEach(p =>
                                {
                                    var key = (p.QQId, p.GroupId);
                                    if(!GroupMembers.ContainsKey(key))
                                        hasUpdate = true;

                                    GroupMembers.AddOrUpdate((p.QQId, p.GroupId), new GroupMember(p),
                                        (ids, updateMember) =>
                                        {
                                            hasUpdate = true;
                                            updateMember.Card = p.Card;
                                            updateMember.PermitType = p.PermitType;
                                            return updateMember;
                                        });
                                });
                            }
                            catch (GroupNotExistsException)
                            {
                                _logger.Info("group[{0}] is not exists anymore, cleaning data", groupId);
                                foreach(var deleteKey in GroupMemberManager.GroupMembers.Keys.Where(p => p.gruopNo == groupId))
                                {
                                    GroupMembers.TryRemove(deleteKey, out _);
                                }
                                hasUpdate = true;
                            }
                        }
                    }
                    catch(Exception e)
                    {
                        _logger.Error(e, "load groupmember error");
                    }

                    if (hasUpdate)
                    {
                        DataManager.Instance.NoticeConfigUpdated();
                    }

                    await Task.Delay(TimeSpan.FromMinutes(5), token);
                }
            });
            return Task.CompletedTask;
        }
    }
}
