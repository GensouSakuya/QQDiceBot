using GensouSakuya.QQBot.Core.PlatformModel;

namespace GensouSakuya.QQBot.Core.Model
{
    public class GuildMember : GuildMemberSourceInfo
    {
        public GuildMember() { }
        public GuildMember(GensouSakuya.QQBot.Core.PlatformModel.GuildMemberSourceInfo memberSourceInfo)
        {
            Copy(memberSourceInfo);
        }

        public void Copy(GensouSakuya.QQBot.Core.PlatformModel.GuildMemberSourceInfo memberSourceInfo)
        {
            GuildId = memberSourceInfo.GuildId;
            UserId = memberSourceInfo.UserId;
            NickName = memberSourceInfo.NickName;
        }
    }
}
