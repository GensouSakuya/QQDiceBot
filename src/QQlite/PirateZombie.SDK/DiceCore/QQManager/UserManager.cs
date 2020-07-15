using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PirateZombie.SDK.BaseModel;
using PirateZombie.SDK;
using PirateZombie.SDK.DiceCore.QQManager;

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
                string result = QLAPI.Api_GetQQInfo(qqNo.ToString(), QLMain.ac);
                if (string.IsNullOrEmpty(result))
                {
                    return null;
                }
                BinaryReader binary = new BinaryReader(new MemoryStream(Convert.FromBase64String(result)));
                var qqInfo = new QQ();
                qqInfo.Id = binary.ReadInt64_Ex();
                qqInfo.Nick = binary.ReadString_Ex(Config.DefaultEncoding);
                qqInfo.Sex = (Sex)binary.ReadInt32_Ex();
                qqInfo.Age = binary.ReadInt32_Ex();
                user = new UserInfo(qqInfo);
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
