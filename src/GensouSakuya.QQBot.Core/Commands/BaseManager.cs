using System.Collections.Generic;
using System.Threading.Tasks;
using GensouSakuya.QQBot.Core.Model;
using GensouSakuya.QQBot.Core.PlatformModel;

namespace net.gensousakuya.dice
{
    public abstract class BaseManager
    {
        public abstract Task ExecuteAsync(List<string> command, List<BaseMessage> originMessage, MessageSourceType sourceType, UserInfo qq, Group group, GroupMember member);
    }
}
