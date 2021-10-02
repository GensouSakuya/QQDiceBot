using System;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using GensouSakuya.QQBot.Core;
using GensouSakuya.QQBot.Core.Base;
using Mirai_CSharp;
using Mirai_CSharp.Models;

namespace GensouSakuya.QQBot.Platform.Mirai
{
    class Program
    {
        private static readonly Logger _logger = Logger.GetLogger<Program>();
        static async Task Main(string[] args)
        {
            Options opt = null;
            Parser.Default.ParseArguments<Options>(args).WithParsed<Options>(o =>
            {
                opt = o;
            });

            var bot = new Bot();

            MiraiHttpSessionOptions options = new MiraiHttpSessionOptions("127.0.0.1", 8080, opt.AuthKey);
            // session 使用 DisposeAsync 模式, 所以使用 await using 自动调用 DisposeAsync 方法。
            // 你也可以不在这里 await using, 不过使用完 session 后请务必调用 DisposeAsync 方法
            await using MiraiHttpSession session = new MiraiHttpSession();
            // 把你实现了 Mirai_CSharp.Plugin.Interfaces 下的接口的类给 new 出来, 然后作为插件塞给 session
            //ExamplePlugin plugin = new ExamplePlugin();
            // 你也可以一个个绑定事件。比如 session.GroupMessageEvt += plugin.GroupMessage;
            // 手动绑定事件后不要再调用AddPlugin, 否则可能导致重复调用
            bot.SetSession(session);
            session.AddPlugin(bot);
            session.DisconnectedEvt += async (s, e) =>
            {
                _logger.Error("disconnected, reconnecting");
                await s.ConnectAsync(options, opt.QQ);
                return true;
            };
            // 使用上边提供的信息异步连接到 mirai-api-http
            await session.ConnectAsync(options, opt.QQ); // 自己填机器人QQ号
            await bot.Start();
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
                    groups.ForEach(p => { session.SendGroupMessageAsync(p, new PlainMessage(message)); });
                }
                else if (readline.StartsWith("togroup ", StringComparison.OrdinalIgnoreCase))
                {
                    var splited = readline.Split(" ");
                    if (!long.TryParse(splited[1], out var groupNo))
                        continue;
                    var message = string.Join(" ", splited.Skip(2));
                    await session.SendGroupMessageAsync(groupNo, new PlainMessage(message));
                }
                else if (readline.Equals("save", StringComparison.OrdinalIgnoreCase))
                {
                    await DataManager.Save();
                }
                else if (readline.Equals("load", StringComparison.OrdinalIgnoreCase))
                {
                    await DataManager.Load();
                }
            }
        }
    }
}
