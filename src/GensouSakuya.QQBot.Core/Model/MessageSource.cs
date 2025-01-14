using GensouSakuya.QQBot.Core.PlatformModel;
using System;

namespace GensouSakuya.QQBot.Core.Model
{
    public class MessageSource
    {
        public MessageSourceType Type { get; private set; }
        public string QQ { get; private set; }
        public Lazy<long?> QQNum { get; private set; }
        public string GroupId { get; private set; }
        public Lazy<long?> GroupIdNum { get; private set; }
        public string GuildId { get; private set; }
        public string ChannelId { get; private set; }

        public dynamic Sender { get; private set; }

        public bool IsTraditionSource => Type != MessageSourceType.Guild;

        private MessageSource() 
        {
            GroupIdNum = new Lazy<long?>(() => long.TryParse(GroupId, out var groupNum) ? groupNum : null);
            QQNum = new Lazy<long?>(() => long.TryParse(QQ, out var qqNum) ? qqNum : null);
        }

        public static MessageSource FromGroup(string userId, string groupId, dynamic sender)
        {
            return new MessageSource
            {
                Type = MessageSourceType.Group,
                GroupId = groupId,
                QQ = userId,
                Sender = sender,
            };
        }

        public static MessageSource FromFriend(string userId, dynamic sender)
        {
            return new MessageSource
            {
                Type = MessageSourceType.Friend,
                QQ = userId,
                Sender = sender,
            };
        }

        public static MessageSource FromGuild(string userId, string guildId, string channelId, dynamic sender)
        {
            return new MessageSource
            {
                Type = MessageSourceType.Guild,
                GuildId = guildId,
                ChannelId = channelId,
                QQ = userId,
                Sender = sender,
            };
        }
    }
}
