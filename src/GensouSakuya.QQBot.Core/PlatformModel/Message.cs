using net.gensousakuya.dice;

namespace GensouSakuya.QQBot.Core.PlatformModel
{
    public class Message
    {
        public MessageSourceType Type { get; set; }
        //public string FromQQ { get; set; }
        public string ToQQ { get; set; }
        public string ToGroup { get; set; }
    }
}
