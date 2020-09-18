using System.Collections.Generic;
using System.Threading.Tasks;
using GensouSakuya.QQBot.Core.PlatformModel;

namespace GensouSakuya.QQBot.Core
{
    public class PlatformManager
    {
        public class Info
        {
            public static QQSourceInfo GetQQInfo(long qq)
            {
                return EventCenter.GetQQInfo?.Invoke(qq);
            }

            public static async Task<GroupMemberSourceInfo> GetGroupoMember(long groupNo, long qq)
            {
                return await EventCenter.GetGroupMember?.Invoke(groupNo, qq);
            }

            public static async Task<List<GroupMemberSourceInfo>> GetGroupMembers(long groupNo)
            {
                return await EventCenter.GetGroupMemberList?.Invoke(groupNo);
            }
        }

        public static void SendGroupMessage(long toGroup, string message)
        {
            EventCenter.SendMessage?.Invoke(new Message
            {
                ToGroup = toGroup,
                Type = MessageSourceType.Group,
                Content = message
            });
        }

        public static void SendToNotFriend(long toQQ, string message)
        {
            EventCenter.SendMessage?.Invoke(new Message
            {
                ToGroup = toQQ,
                Type = MessageSourceType.Private,
                Content = message
            });
        }
        public static void SendToFriend(long toQQ, string message)
        {
            EventCenter.SendMessage?.Invoke(new Message
            {
                ToGroup = toQQ,
                Type = MessageSourceType.Friend,
                Content = message
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
