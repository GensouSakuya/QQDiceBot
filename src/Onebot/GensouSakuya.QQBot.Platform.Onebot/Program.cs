
namespace GensouSakuya.QQBot.Platform.Onebot
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            if(args.Length <2) 
            {
                Console.WriteLine(".\\bot.exe [onebotHost] [qq]");
                return;
            }
            var host = args[0];
            var qq = args[1];
            var bot = new Bot(host);
            await bot.Start(long.TryParse(qq, out var qqNo) ? qqNo : throw new InvalidOperationException("invalid qq"));

            while (true)
            {
                var readline = await Console.In.ReadLineAsync();
                if (readline == "exit")
                {
                    return;
                }
                else if (readline.StartsWith("notice ", StringComparison.OrdinalIgnoreCase))
                {
                    var message = $"[֪ͨ]{readline.Substring(7)}";
                    var groups = GensouSakuya.QQBot.Core.QQManager.GroupMemberManager.GroupMembers.Values
                        .Select(p => p.GroupId).Distinct().ToList();
                    groups.ForEach(p => { bot.OneBot.SendGroupMessage(p, message); });
                }
                else if (readline.StartsWith("togroup ", StringComparison.OrdinalIgnoreCase))
                {
                    var splited = readline.Split(" ");
                    if (!long.TryParse(splited[1], out var groupNo))
                        continue;
                    var message = string.Join(" ", splited.Skip(2));
                    await bot.OneBot.SendGroupMessage(groupNo, message);
                }
            }
        }
    }
}
