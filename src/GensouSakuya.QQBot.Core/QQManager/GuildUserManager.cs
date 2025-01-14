using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using GensouSakuya.QQBot.Core.Base;
using GensouSakuya.QQBot.Core.Model;

namespace GensouSakuya.QQBot.Core.QQManager
{
    public class GuildUserManager
    {
        private static readonly Logger _logger = Logger.GetLogger<GuildUserManager>();
        public static ConcurrentDictionary<string, GuildUserInfo> Users { get; set; } = new ConcurrentDictionary<string, GuildUserInfo>();
        public static async Task<GuildUserInfo> Get(string userId, string guildId)
        {
            if (Users.TryGetValue(userId, out var user))
                return user;

            var source = await PlatformManager.Info.GetGuildMember(userId, guildId);
            if (source == null)
                return null;

            user = new GuildUserInfo(source);

            return Add(user);
        }

        public static GuildUserInfo Add(GuildUserInfo qqInfo)
        {
            var hasUpdate = false;
            if (!Users.ContainsKey(qqInfo.Id))
                hasUpdate = true;

            Users.AddOrUpdate(qqInfo.Id, new GuildUserInfo(qqInfo), (key, source) =>
            {
                //if(source.Nick!= qqInfo.Nick || source.Sex != qqInfo.Sex)
                //{
                //    hasUpdate = true;
                //    source.Nick = qqInfo.Nick;
                //    source.Sex = qqInfo.Sex;
                //}
                return source;
            });

            if(hasUpdate)
                DataManager.NoticeConfigUpdatedAction();

            return Users.TryGetValue(qqInfo.Id, out var user) ? user : null;
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
            //            foreach (var qq in Users.Keys)
            //            {
            //                try
            //                {
            //                    var qqInfo = PlatformManager.Info.GetQQInfo(qq);
            //                    if (qqInfo == null)
            //                        continue;

            //                    if (!Users.ContainsKey(qq))
            //                        hasUpdate = true;
            //                    Users.AddOrUpdate(qq, new UserInfo(qqInfo), (key, source) =>
            //                    {
            //                        if (source.Nick != qqInfo.Nick || source.Sex != qqInfo.Sex)
            //                        {
            //                            hasUpdate = true;
            //                            source.Nick = qqInfo.Nick;
            //                            source.Sex = qqInfo.Sex;
            //                        }
            //                        return source;
            //                    });
            //                }
            //                catch (QQNotExistsException)
            //                {
            //                    _logger.Info("user[{0}] is not exists anymore, cleaning data", qq);
            //                    Users.TryRemove(qq, out _);
            //                    hasUpdate = true;
            //                }
            //            }
            //        }
            //        catch (Exception e)
            //        {
            //            _logger.Error(e, "load usres error");
            //        }

            //        if (hasUpdate)
            //        {
            //            DataManager.NoticeConfigUpdatedAction();
            //        }

            //        await Task.Delay(TimeSpan.FromMinutes(5), token);
            //    }
            //});
            return Task.CompletedTask;
        }

    }
}
