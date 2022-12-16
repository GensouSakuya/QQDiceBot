using System;
using GensouSakuya.QQBot.Core.Interfaces;
using GensouSakuya.QQBot.Core.PlatformModel;

namespace GensouSakuya.QQBot.Core.Model
{
    public class UserInfo: QQSourceInfo, IUserJrrp
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

        #endregion
    }
}
