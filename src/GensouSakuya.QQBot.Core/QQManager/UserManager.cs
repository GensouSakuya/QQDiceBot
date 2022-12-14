using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using GensouSakuya.QQBot.Core.Base;
using GensouSakuya.QQBot.Core.Exceptions;
using GensouSakuya.QQBot.Core.Model;
using GensouSakuya.QQBot.Core.PlatformModel;

namespace GensouSakuya.QQBot.Core.QQManager
{
    public class UserManager
    {
        private static readonly Logger _logger = Logger.GetLogger<UserManager>();
        public static ConcurrentDictionary<long,UserInfo> Users { get; set; } = new ConcurrentDictionary<long,UserInfo>();
        public static UserInfo Get(long qq)
        {
            if (Users.TryGetValue(qq, out var user))
                return user;

            var qqInfo = PlatformManager.Info.GetQQInfo(qq);

            return Add(qqInfo);
        }

        public static UserInfo Add(QQSourceInfo qqInfo)
        {
            var hasUpdate = false;
            if (!Users.ContainsKey(qqInfo.Id))
                hasUpdate = true;

            Users.AddOrUpdate(qqInfo.Id, new UserInfo(qqInfo), (key, source) =>
            {
                if(source.Nick!= qqInfo.Nick || source.Sex != qqInfo.Sex)
                {
                    hasUpdate = true;
                    source.Nick = qqInfo.Nick;
                    source.Sex = qqInfo.Sex;
                }
                return source;
            });

            if(hasUpdate)
                DataManager.Instance.NoticeConfigUpdated();

            return Users.TryGetValue(qqInfo.Id, out var user) ? user : null;
        }

        public static Task StartLoadTask(CancellationToken token = default)
        {
            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    var hasUpdate = false;
                    try
                    {
                        foreach (var qq in Users.Keys)
                        {
                            try
                            {
                                var qqInfo = PlatformManager.Info.GetQQInfo(qq);
                                if (qqInfo == null)
                                    continue;

                                if (!Users.ContainsKey(qq))
                                    hasUpdate = true;
                                Users.AddOrUpdate(qq, new UserInfo(qqInfo), (key, source) =>
                                {
                                    if (source.Nick != qqInfo.Nick || source.Sex != qqInfo.Sex)
                                    {
                                        hasUpdate = true;
                                        source.Nick = qqInfo.Nick;
                                        source.Sex = qqInfo.Sex;
                                    }
                                    return source;
                                });
                            }
                            catch (QQNotExistsException)
                            {
                                _logger.Info("user[{0}] is not exists anymore, cleaning data", qq);
                                Users.TryRemove(qq, out _);
                                hasUpdate = true;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "load usres error");
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
