using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using GensouSakuya.QQBot.Core.Base;
using GensouSakuya.QQBot.Core.Model;

namespace GensouSakuya.QQBot.Core.QQManager
{
    public class GuildMemberManager
    {
        private static readonly Logger _logger = Logger.GetLogger<GuildMemberManager>();
        public static ConcurrentDictionary<(string userId, string guildId), GuildMember> GuildMembers = new ConcurrentDictionary<(string, string), GuildMember>();

        public static async Task<GuildMember> Get(string userId, string guildId)
        {
            if (GuildMembers.TryGetValue((userId, guildId), out var member))
                return member;

            var sourceMember = await PlatformManager.Info.GetGuildMember(userId, guildId);
            if (sourceMember == null)
                return null;

            var hasUpdate = false;
            GuildMembers.AddOrUpdate((sourceMember.UserId, sourceMember.GuildId), new GuildMember(sourceMember), (ids, updateMember) =>
            {
                if (updateMember.NickName != sourceMember.NickName)
                {
                    hasUpdate = true;
                    updateMember.NickName = sourceMember.NickName;
                }
                return updateMember;
            });

            if (hasUpdate)
                DataManager.Instance.NoticeConfigUpdated();

            return GuildMembers.TryGetValue((userId, guildId), out member) ? member : null;
        }

        public static void UpdateNickName(GuildMember member, string name)
        {
            if (name == null)
                return;

            if(member.NickName != name)
            {
                GuildMembers.AddOrUpdate((member.UserId, member.GuildId), new GuildMember(member) {
                    NickName = name
                }, (ids, updateMember) =>
                {
                    if (updateMember.NickName != name)
                    {
                        updateMember.NickName = name;
                    }
                    return updateMember;
                });
                DataManager.Instance.NoticeConfigUpdated();
            }
        }

        public static Task StartLoadTask(CancellationToken token = default)
        {
            //Task.Run(async () =>
            //{
            //    while (!token.IsCancellationRequested)
            //    {
            //        var hasUpdate = false;
            //        try
            //        {
            //            var groupIds = GuildMembers.Values.Select(p => p.GuildId).Distinct();
            //            foreach (var groupId in groupIds)
            //            {
            //                try
            //                {
            //                    var sourceMembers = await PlatformManager.Info.GetGroupMembers(groupId);
            //                    if (sourceMembers == null)
            //                        continue;

            //                    sourceMembers?.ForEach(p =>
            //                    {
            //                        var key = (p.QQId, p.GroupId);
            //                        if (!GuildMembers.ContainsKey(key))
            //                            hasUpdate = true;

            //                        GuildMembers.AddOrUpdate((p.QQId, p.GroupId), new GroupMember(p),
            //                            (ids, updateMember) =>
            //                            {
            //                                if (updateMember.Card != p.Card || updateMember.PermitType != p.PermitType)
            //                                {
            //                                    hasUpdate = true;
            //                                    updateMember.Card = p.Card;
            //                                    updateMember.PermitType = p.PermitType;
            //                                }
            //                                return updateMember;
            //                            });
            //                    });
            //                }
            //                catch (GroupNotExistsException)
            //                {
            //                    _logger.Info("group[{0}] is not exists anymore, cleaning data", groupId);
            //                    foreach (var deleteKey in GroupMemberManager.GroupMembers.Keys.Where(p =>
            //                        p.gruopNo == groupId))
            //                    {
            //                        GuildMembers.TryRemove(deleteKey, out _);
            //                    }

            //                    hasUpdate = true;
            //                }
            //            }
            //        }
            //        catch (Exception e)
            //        {
            //            _logger.Error(e, "load groupmember error");
            //        }

            //        if (hasUpdate)
            //        {
            //            DataManager.Instance.NoticeConfigUpdated();
            //        }

            //        await Task.Delay(TimeSpan.FromMinutes(5), token);
            //    }
            //}, CancellationToken.None);
            return Task.CompletedTask;
        }
    }
}
