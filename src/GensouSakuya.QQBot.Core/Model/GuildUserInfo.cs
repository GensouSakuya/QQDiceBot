using GensouSakuya.QQBot.Core.Interfaces;
using System;
using GensouSakuya.QQBot.Core.PlatformModel;

namespace GensouSakuya.QQBot.Core.Model
{
    public class GuildUserInfo: IUserJrrp
    {
        public string Id { get; set; }

        public GuildUserInfo() { }

        public GuildUserInfo(GuildUserInfo qqSourceInfo)
        {
            Id = qqSourceInfo.Id;
        }

        public GuildUserInfo(GuildMemberSourceInfo source)
        {
            Id = source.UserId;
        }

        #region Jrrp

        public DateTime? LastJrrpDate { get; set; }

        public int Jrrp { get; set; } = -1;

        public RerollStep ReRollStep { get; set; } = RerollStep.None;

        #endregion
    }
}
