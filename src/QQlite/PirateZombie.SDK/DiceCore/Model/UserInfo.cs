using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PirateZombie.SDK.BaseModel;

namespace net.gensousakuya.dice
{
    public class UserInfo: QQ
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

        public UserInfo(QQ qq)
        {
            Id = qq.Id;
            Nick = qq.Nick;
            Sex = qq.Sex;
            Age = qq.Age;
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
