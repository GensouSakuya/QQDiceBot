using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PirateZombie.SDK.BaseModel;
using PirateZombie.SDK;
using PirateZombie.SDK.DiceCore.QQManager;
using Newtonsoft.Json;

namespace net.gensousakuya.dice
{
    public static class UserManager
    {
        public static List<UserInfo> Users { get; set; } = new List<UserInfo>();
        public static UserInfo Get(long qqNo)
        {
            string result = QLAPI.Api_GetQQInfo(qqNo.ToString(), QLMain.ac);

            //QLAPI.Api_SendLog("Debug", "Api_GetQQInfo:"+result, 0, QLMain.ac);
            if (string.IsNullOrEmpty(result))
            {
                return null;
            }

            var info = JsonConvert.DeserializeObject<JsonModel<QQJsonModel>>(result);
            var qq = info.Result.buddy.info_list[0];

            var qqInfo = new QQ();
            qqInfo.Id = qq.uin;
            qqInfo.Nick = qq.nick;
            qqInfo.Sex = (Sex) qq.gender;

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

    public class JsonModel<T>
    {
        public int retcode { get; set; }
        public T Result { get; set; }
    }

    public class QQJsonModel
    {
        public class infoModel
        {
            public int gender_id { get; set; }
            public string lnick { get; set; }
            public long uin { get; set; }
            public int gender { get; set; }
            public string nick { get; set; }
        }
        public class buddyModel
        {
            public List<infoModel> info_list { get; set; }
        }
        public buddyModel buddy { get; set; }
    }
}
