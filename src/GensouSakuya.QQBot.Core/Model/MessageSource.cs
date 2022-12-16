using GensouSakuya.QQBot.Core.PlatformModel;

namespace GensouSakuya.QQBot.Core.Model
{
    public class MessageSource
    {
        public MessageSourceType Type { get; private set; }
        public string QQ { get; private set; }
        public string GroupId { get; private set; }
        public string GuildId { get; private set; }
        public string ChannelId { get; private set; }

        public bool IsTraditionSource => Type != MessageSourceType.Guild;

        private MessageSource() { }

        public static MessageSource FromGroup(string userId, string groupId)
        {
            return new MessageSource
            {
                Type = MessageSourceType.Group,
                GroupId = groupId,
                QQ = userId,
            };
        }

        public static MessageSource FromFriend(string userId)
        {
            return new MessageSource
            {
                Type = MessageSourceType.Friend,
                QQ = userId,
            };
        }

        public static MessageSource FromGuild(string userId, string guildId, string channelId)
        {
            return new MessageSource
            {
                Type = MessageSourceType.Guild,
                GuildId = guildId,
                ChannelId = channelId,
                QQ = userId,
            };
        }
    }
}
