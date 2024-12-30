using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GensouSakuya.QQBot.Core.Exceptions;
using GensouSakuya.QQBot.Core.PlatformModel;

namespace GensouSakuya.QQBot.Core
{
    public class PlatformManager
    {
        private static readonly Logger _logger = Logger.GetLogger<PlatformManager>();
        public class Info
        {
            public static async Task<QQSourceInfo> GetQQInfo(long qq)
            {
                if (EventCenter.GetQQInfo == null)
                    return null;
                //后续逻辑若资源不存在应该抛出对应的异常
                var res = await EventCenter.GetQQInfo?.Invoke(qq);
                if(res == null)
                {
                    throw new QQNotExistsException(qq);
                }
                return res;
            }

            public static async Task<GroupMemberSourceInfo> GetGroupoMember(long groupNo, long qq)
            {
                if (EventCenter.GetGroupMember == null)
                    return null;

                //后续逻辑若资源不存在应该抛出对应的异常
                var res = await EventCenter.GetGroupMember?.Invoke(groupNo, qq);
                if (res == null)
                {
                    throw new GroupMemberNotExistsException(groupNo, qq);
                }
                return res;
            }

            public static async Task<List<GroupMemberSourceInfo>> GetGroupMembers(long groupNo)
            {
                if (EventCenter.GetGroupMemberList == null)
                    return null;
                //后续逻辑若资源不存在应该抛出对应的异常
                var res = await EventCenter.GetGroupMemberList?.Invoke(groupNo);
                if (res == null)
                {
                    throw new GroupNotExistsException(groupNo);
                }
                return res;
            }

            public static async Task<GuildMemberSourceInfo> GetGuildMember(string userId,string guildId)
            {
                if (EventCenter.GetGuildMember == null)
                    return null;
                //后续逻辑若资源不存在应该抛出对应的异常
                var res = await EventCenter.GetGuildMember?.Invoke(userId,guildId);
                if (res == null)
                {
                    throw new GuildMemberNotExistsException(userId, guildId);
                }
                return res;
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
    }
}
