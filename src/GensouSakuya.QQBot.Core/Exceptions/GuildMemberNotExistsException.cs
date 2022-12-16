using System;

namespace GensouSakuya.QQBot.Core.Exceptions
{
    public class GuildMemberNotExistsException : Exception
    {
        public GuildMemberNotExistsException(string userId, string guildId)
        {
            UserId = userId;
            GuildId = guildId;
        }

        public string UserId { get; private set; }
        public string GuildId { get; private set; }
    }
}
