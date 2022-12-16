namespace GensouSakuya.QQBot.Core.Model
{
    public class GroupMember:PlatformModel.GroupMemberSourceInfo
    {

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
        }

        public string NickName { get; set; }

        public GroupMember() { }
        public GroupMember(GensouSakuya.QQBot.Core.PlatformModel.GroupMemberSourceInfo memberSourceInfo)
        {
            Copy(memberSourceInfo);
        }

        public void Copy(GensouSakuya.QQBot.Core.PlatformModel.GroupMemberSourceInfo memberSourceInfo)
        {
            Card = memberSourceInfo.Card;
            GroupId = memberSourceInfo.GroupId;
            PermitType = memberSourceInfo.PermitType;
            QQId = memberSourceInfo.QQId;
        }
    }
}
