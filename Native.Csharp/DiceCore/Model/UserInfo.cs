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

        private Dictionary<string, string> GroupNames = new Dictionary<string, string>();

        private DateTime? LastJrrpDate { get; set; }

        private int _jrrp = -1;

        public int? Jrrp
        {
            get
            {
                if (LastJrrpDate.HasValue && LastJrrpDate == DateTime.Today)
                {
                    return _jrrp;
                }
                return null;
            }
            set
            {
                if (!LastJrrpDate.HasValue || LastJrrpDate != DateTime.Today)
                {
                    LastJrrpDate = DateTime.Today;
                    _jrrp = value ?? -1;
                }
            }
        }

        public UserInfo(Native.Csharp.Sdk.Cqp.Model.QQ qq)
        {
            Id = qq.Id;
            Nick = qq.Nick;
            Sex = qq.Sex;
            Age = qq.Age;
        }
    }
}
