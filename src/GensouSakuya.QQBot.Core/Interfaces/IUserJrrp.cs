using System;

namespace GensouSakuya.QQBot.Core.Interfaces
{
    internal interface IUserJrrp
    {
        DateTime? LastJrrpDate { get; set; }

        int Jrrp { get; set; }

        RerollStep ReRollStep { get; set; }
    }

    public enum RerollStep
    {
        None = 0,
        CanReroll = 1,
        RerollSuccess = 2,
        RerollFaild = 3,
        RerollDevastated = 4,
    }
}
