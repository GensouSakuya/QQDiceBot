using PirateZombie.SDK.BaseModel;

namespace GensouSakuya.QQBot.Core.PlatformModel
{
	/// <summary>
	/// 群成员信息
	/// </summary>
	public class GroupMember
	{
		/// <summary>
		/// 获取或设置一个值, 指示成员所在群
		/// </summary>
		public long GroupId { get; set; }
		/// <summary>
		/// 获取或设置一个值, 指示成员QQ
		/// </summary>
		public long QQId { get; set; }
		/// <summary>
		/// 获取或设置一个值, 指示成员昵称
		/// </summary>
		//public string Nick { get; set; }
		/// <summary>
		/// 获取或设置一个值, 指示成员群名片
		/// </summary>
		public string Card { get; set; }
		///// <summary>
		///// 获取或设置一个值, 指示成员性别
		///// </summary>
		//public Sex Sex { get; set; }
		///// <summary>
		///// 获取或设置一个值, 指示成员年龄
		///// </summary>
		//public int Age { get; set; }
		///// <summary>
		///// 获取或设置一个值, 指示成员所在地区
		///// </summary>
		//public string Area { get; set; }
		///// <summary>
		///// 获取或设置一个值, 指示成员入群的时间
		///// </summary>
		//public DateTime JoiningTime { get; set; }
		///// <summary>
		///// 获取或设置一个值, 指示成员最后发言时间
		///// </summary>
		//public DateTime LastDateTime { get; set; }
		///// <summary>
		///// 获取或设置一个值, 指示成员等级的名称
		///// </summary>
		//public string Level { get; set; }
		/// <summary>
		/// 获取或设置一个值, 指示成员在此群的权限
		/// </summary>
		public PermitType PermitType { get; set; }
		/// <summary>
		///// 获取或设置一个值, 指示成员在此群获得的专属头衔
		///// </summary>
		//public string SpecialTitle { get; set; }
		///// <summary>
		///// 获取或设置一个值, 指示成员在此群的专属头衔过期时间, 若过期时间换算的秒数为 "-1" 则为无期限
		///// </summary>
		//public DateTime SpecialTitleDurationTime { get; set; }
		///// <summary>
		///// 获取或设置一个值, 指示成员是否为不良记录成员
		///// </summary>
		//public bool BadRecord { get; set; }
		///// <summary>
		///// 获取或设置一个值, 指示成员是否被允许修改群名片
		///// </summary>
		//public bool CanModifiedCard { get; set; }


        public long GroupNumber
        {
            get => this.GroupId;
            set => this.GroupId = value;
        }

        public long QQ
        {
            get => this.QQId;
            set => this.QQId = value;
        }

        public string GroupName
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(NickName))
                {
                    return NickName;
                }
                return Card;
            }
            set => Card = value;
        }

        public string NickName { get; set; }

        public GroupMember() { }
        public GroupMember(GensouSakuya.QQBot.Core.PlatformModel.GroupMember member)
        {
            Copy(member);
        }

        public void Copy(GensouSakuya.QQBot.Core.PlatformModel.GroupMember member)
        {
            Card = member.Card;
            GroupId = member.GroupId;
            PermitType = member.PermitType;
            QQId = member.QQId;
        }
	}
}
