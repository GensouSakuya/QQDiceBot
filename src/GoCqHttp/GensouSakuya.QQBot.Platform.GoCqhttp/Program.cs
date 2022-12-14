using System;
using System.Linq;
using System.Threading.Tasks;

namespace GensouSakuya.QQBot.Platform.GoCqhttp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var bot = new Bot("127.0.0.1", 8080);
            await bot.Start(args[0]);

            while (true)
            {
                var readline = await Console.In.ReadLineAsync();
                if (readline == "exit")
                {
                    return;
                }
                else if (readline.StartsWith("notice ", StringComparison.OrdinalIgnoreCase))
                {
                    var message = $"[通知]{readline.Substring(7)}";
                    var groups = GensouSakuya.QQBot.Core.QQManager.GroupMemberManager.GroupMembers.Values
                        .Select(p => p.GroupId).Distinct().ToList();
                    groups.ForEach(p => { bot.Session.SendGroupMessage(p.ToString(), message); });
                }
                else if (readline.StartsWith("togroup ", StringComparison.OrdinalIgnoreCase))
                {
                    var splited = readline.Split(" ");
                    if (!long.TryParse(splited[1], out var groupNo))
                        continue;
                    var message = string.Join(" ", splited.Skip(2));
                    await bot.Session.SendGroupMessage(groupNo.ToString(), message);
                }
            }
        }
    }
}
