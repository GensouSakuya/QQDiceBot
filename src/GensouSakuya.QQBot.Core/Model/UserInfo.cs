using System;
using GensouSakuya.QQBot.Core.PlatformModel;

namespace GensouSakuya.QQBot.Core.Model
{
    public class UserInfo: QQSourceInfo
    {
        public long QQ
        {
            get { return this.Id;}
            set { Id = value; }
        }

        public string Name
        {
            get { return this.Nick; }
            set { Nick = value; }
        }

        public UserInfo() { }

        public UserInfo(QQSourceInfo qqSourceInfo)
        {
            Id = qqSourceInfo.Id;
            Nick = qqSourceInfo.Nick;
            Sex = qqSourceInfo.Sex;
        }

        #region Jrrp

        public DateTime? LastJrrpDate { get; set; }

        public int Jrrp { get; set; } = -1;

        public RerollStep ReRollStep { get; set; } = RerollStep.None;

        public enum RerollStep
        {
            None = 0,
            CanReroll = 1,
            RerollSuccess = 2,
            RerollFaild = 3,
            RerollDevastated = 4,
        }
        #endregion
    }
}
