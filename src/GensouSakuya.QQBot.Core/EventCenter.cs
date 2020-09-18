using System;
using System.Collections.Generic;
using System.Text;
using GensouSakuya.QQBot.Core.PlatformModel;

namespace GensouSakuya.QQBot.Core
{
    public class EventCenter
    {
        public static Action<Log> Log { get; set; }

        public static Action<Message> SendMessage { get; set; }

        public static Func<string, List<GroupMember>> GetGroupMemberList { get; set; }

        public static Func<string,QQ> GetQQInfo { get; set; }
    }
}
