using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GensouSakuya.QQBot.Core.PlatformModel
{
    public class PlatformApiModel
    {
        public Func<Message, Task> SendMessage { get; set; }
        public Func<long, Task<List<GroupMemberSourceInfo>>> GetGroupMemberList { get; set; }
        public Func<long, long, Task<GroupMemberSourceInfo>> GetGroupMember { get; set; }
        public Func<string, string, Task<GuildMemberSourceInfo>> GetGuildMember { get; set; }
        public Func<long, Task<List<BaseMessage>>> GetMessageById { get; set; }
    }
}
