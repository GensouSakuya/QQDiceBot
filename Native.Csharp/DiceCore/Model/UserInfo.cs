using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace net.gensousakuya.dice
{
    public class UserInfo: Native.Csharp.Sdk.Cqp.Model.QQ
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

        public UserInfo(Native.Csharp.Sdk.Cqp.Model.QQ qq)
        {
            Id = qq.Id;
            Nick = qq.Nick;
            Sex = qq.Sex;
            Age = qq.Age;
        }

        #region Jrrp

        public DateTime? LastJrrpDate { get; set; }

        public int JrrpDurationDays { get; set; }

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
