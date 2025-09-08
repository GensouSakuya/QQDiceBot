using GensouSakuya.QQBot.Core;
using GensouSakuya.QQBot.Core.Base;

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
            var apiHost = args[0];
            var qq = args[1];
            var bc = new BaseConfig
            {
                Host = apiHost,
                QQ = long.TryParse(qq, out var qqNo) ? qqNo : throw new InvalidOperationException("invalid qq")
            };
            var builder = WebApplication.CreateBuilder();
            builder.Services.AddQQBot(builder.Configuration, bc);
            builder.Services.AddHostedService<Bot>();
            var host = builder.Build();

            host.UseQQBot();
            Console.WriteLine($"current Env:{host.Environment.EnvironmentName}");

            await host.RunAsync();

            //while (true)
            //{
            //    var readline = await Console.In.ReadLineAsync();
            //    if (readline == "exit")
            //    {
            //        return;
            //    }
            //    else if (readline.StartsWith("notice ", StringComparison.OrdinalIgnoreCase))
            //    {
            //        var message = $"[֪ͨ]{readline.Substring(7)}";
            //        var groups = GensouSakuya.QQBot.Core.QQManager.GroupMemberManager.GroupMembers.Values
            //            .Select(p => p.GroupId).Distinct().ToList();
            //        groups.ForEach(p => { bot.OneBot.SendGroupMessage(p, message); });
            //    }
            //    else if (readline.StartsWith("togroup ", StringComparison.OrdinalIgnoreCase))
            //    {
            //        var splited = readline.Split(" ");
            //        if (!long.TryParse(splited[1], out var groupNo))
            //            continue;
            //        var message = string.Join(" ", splited.Skip(2));
            //        await bot.OneBot.SendGroupMessage(groupNo, message);
            //    }
            //}
        }
    }
}
