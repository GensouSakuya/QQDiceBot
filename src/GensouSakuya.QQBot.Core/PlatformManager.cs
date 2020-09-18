using System.Collections.Generic;
using GensouSakuya.QQBot.Core.PlatformModel;
using net.gensousakuya.dice;

namespace GensouSakuya.QQBot.Core
{
    public class PlatformManager
    {
        public class Info
        {
            public static QQSourceInfo GetQQInfo(string qq)
            {
                return EventCenter.GetQQInfo?.Invoke(qq);
            }

            public static List<GroupMemberSourceInfo> GetGroupMembers(string groupNo)
            {
                return EventCenter.GetGroupMemberList?.Invoke(groupNo);
            }
        }

        public static void SendGroupMessage(string toGroup, string message)
        {
            EventCenter.SendMessage?.Invoke(new Message
            {
                ToGroup = toGroup,
                Type = MessageSourceType.Group,
            });
        }

        public static void SendToNotFriend(string toQQ, string message)
        {
            EventCenter.SendMessage?.Invoke(new Message
            {
                ToGroup = toQQ,
                Type = MessageSourceType.Private,
            });
        }
        public static void SendToFriend(string toQQ, string message)
        {
            EventCenter.SendMessage?.Invoke(new Message
            {
                ToGroup = toQQ,
                Type = MessageSourceType.Friend,
            });
        }


        public class Log
        {
            public static void Debug(string message)
            {
                EventCenter.Log?.Invoke(new PlatformModel.Log(message, LogLevel.Debug));
            }
            public static void Error(string message)
            {
                EventCenter.Log?.Invoke(new PlatformModel.Log(message, LogLevel.Error));
            }
        }
    }
}
