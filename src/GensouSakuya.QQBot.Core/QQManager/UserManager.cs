using System.Collections.Generic;
using System.Linq;
using GensouSakuya.QQBot.Core.Model;
using GensouSakuya.QQBot.Core.PlatformModel;

namespace GensouSakuya.QQBot.Core.QQManager
{
    public static class UserManager
    {
        public static List<UserInfo> Users { get; set; } = new List<UserInfo>();
        public static UserInfo Get(long qqNo)
        {
            var user = Users.Find(p => p.QQ == qqNo);
            if (user != null)
                return user;

            var qqInfo = PlatformManager.Info.GetQQInfo(qqNo);

            return Add(qqInfo);
        }

        private static readonly object AddLock = new object();

        public static UserInfo Add(QQSourceInfo user)
        {
            var source = Users.Find(p => p.QQ == user.Id);
            if (source != null)
            {
                source.Nick = user.Nick;
                source.Sex = user.Sex;
            }
            else
            {
                lock (AddLock)
                {
                    if (Users.All(p => p.QQ != user.Id))
                    {
                        source = new UserInfo(user);
                        Users.Add(source);
                    }
                }
            }
            return source;
        }
    }
}
