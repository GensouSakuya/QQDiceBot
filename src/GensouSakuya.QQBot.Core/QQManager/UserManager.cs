using System.Collections.Generic;
using GensouSakuya.QQBot.Core.Model;

namespace GensouSakuya.QQBot.Core.QQManager
{
    public static class UserManager
    {
        public static List<UserInfo> Users { get; set; } = new List<UserInfo>();
        public static UserInfo Get(long qqNo)
        {
            var qqInfo = PlatformManager.Info.GetQQInfo(qqNo.ToString());

            var user = Users.Find(p => p.QQ == qqNo);
            if (user != null)
            {
                user.Nick = qqInfo.Nick;
                user.Sex = qqInfo.Sex;
            }
            else
            {
                Users.Add(new UserInfo(qqInfo));
            }

            return user;
        }
    }
}
