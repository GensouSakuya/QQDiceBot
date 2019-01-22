namespace net.gensousakuya.dice
{
    public class GroupMember: Native.Csharp.Sdk.Cqp.Model.GroupMember
    {
        public long GroupNumber
        {
            get { return this.GroupId; }
            set { this.GroupId = value; }
        }

        public long QQ
        {
            get { return this.QQId; }
            set { this.QQId = value; }
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
            set { Card = value; }
        }

        public string NickName { get; set; }

        public GroupMember() { }
        public GroupMember(Native.Csharp.Sdk.Cqp.Model.GroupMember member)
        {
            Age = member.Age;
            Area = member.Area;
            BadRecord = member.BadRecord;
            CanModifiedCard = member.CanModifiedCard;
            Card = member.Card;
            GroupId = member.GroupId;
            JoiningTime = member.JoiningTime;
            LastDateTime = member.LastDateTime;
            Level = member.Level;
            Nick = member.Nick;
            PermitType = member.PermitType;
            QQId = member.QQId;
            Sex = member.Sex;
            SpecialTitle = member.SpecialTitle;
            SpecialTitleDurationTime = member.SpecialTitleDurationTime;
        }
    }
}
