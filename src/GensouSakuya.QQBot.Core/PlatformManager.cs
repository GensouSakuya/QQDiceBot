using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GensouSakuya.QQBot.Core.PlatformModel;

namespace GensouSakuya.QQBot.Core
{
    public class PlatformManager
    {
        private static readonly Logger _logger = Logger.GetLogger<PlatformManager>();
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

        public static void SendMessage(Message message)
        {
            try
            {
                EventCenter.SendMessage?.Invoke(message);
            }
            catch(Exception e)
            {
                _logger.Error(e, "send message error");
            }
        }

        //输出到外部UI用的logger，有需要时再考虑接入
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
