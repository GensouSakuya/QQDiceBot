namespace net.gensousakuya.dice
{
    public class GroupMember: PirateZombie.SDK.BaseModel.GroupMember
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
            set => Card = value;
        }

        public string NickName { get; set; }

        public GroupMember() { }
        public GroupMember(PirateZombie.SDK.BaseModel.GroupMember member)
        {
            Copy(member);
        }

        public void Copy(PirateZombie.SDK.BaseModel.GroupMember member)
        {
            Card = member.Card;
            GroupId = member.GroupId;
            PermitType = member.PermitType;
            QQId = member.QQId;
        }
    }
}
