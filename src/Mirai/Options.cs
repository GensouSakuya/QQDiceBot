using CommandLine;

namespace GensouSakuya.QQBot.Platform.Mirai
{
    public class Options
    {
        [Option("qq")]
        public long QQ { get; set; }
        [Option("authKey")]
        public string AuthKey { get; set; }
    }
}
