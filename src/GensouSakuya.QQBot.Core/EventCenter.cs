using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GensouSakuya.QQBot.Core.PlatformModel;

namespace GensouSakuya.QQBot.Core
{
    public class EventCenter
    {
        public static Action<Log> Log { get; set; }

        public static Action<Message> SendMessage { get; set; }

        public static Func<long, Task<List<GroupMemberSourceInfo>>> GetGroupMemberList { get; set; }
        public static Func<long,long, Task<GroupMemberSourceInfo>> GetGroupMember { get; set; }

        public static Func<long, QQSourceInfo> GetQQInfo { get; set; }
    }
}
