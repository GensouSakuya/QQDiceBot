namespace GensouSakuya.QQBot.Core.Model
{
    public class SourceFullInfo
    {
        public UserInfo QQ { get; set; }
        public Group Group { get; set; }
        public  GroupMember GroupMember { get; set; }
        public GuildUserInfo GuildUser { get; set; }
        public GuildMember GuildMember { get; set; }
    }
}
