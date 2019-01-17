using Native.Csharp.App;
using Native.Csharp.Sdk.Cqp.Api;
using Native.Csharp.Sdk.Cqp.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace net.gensousakuya.dice
{
    public static class UserManager
    {
        private static List<UserInfo> _users
        {
            get { return DataManager.Instance.Users; }
        }

        public static UserInfo Get(long qqNo)
        {
            var user = _users.Find(p => p.QQ == qqNo);
            if(user == null)
            {
                Common.CqApi.GetQQInfo(qqNo, out QQ qq);
                user = new UserInfo(qq);
                _users.Add(user);
            }

            return user;
        }
        public static UserInfo Add(UserInfo qq)
        {
            if (_users.Any(p => p.QQ == qq.QQ))
                return _users.Find(p => p.QQ == qq.QQ);
            else
            {
                _users.Add(qq);
                return qq;
            }
        }
    }
}
