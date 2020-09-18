
namespace GensouSakuya.QQBot.Core.PlatformModel
{
    public class Message
    {
        public MessageSourceType Type { get; set; }
        //public string FromQQ { get; set; }
        public long ToQQ { get; set; }
        public long ToGroup { get; set; }
        public string Content { get; set; }
    }
}
