using System;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using GensouSakuya.QQBot.Core;
using GensouSakuya.QQBot.Core.Base;
using Microsoft.Extensions.DependencyInjection;
using Mirai.CSharp.Builders;
using Mirai.CSharp.HttpApi.Builder;
using Mirai.CSharp.HttpApi.Invoking;
using Mirai.CSharp.HttpApi.Models.ChatMessages;
using Mirai.CSharp.HttpApi.Options;
using Mirai.CSharp.HttpApi.Session;

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
            IServiceProvider services = new ServiceCollection().AddMiraiBaseFramework() // 表示使用基于基础框架的构建器
                .AddHandler<Bot>(p=> bot, ServiceLifetime.Singleton)
                .Services
                .AddDefaultMiraiHttpFramework() // 表示使用 mirai-api-http 实现的构建器
                .ResolveParser<Bot>(ServiceLifetime.Singleton)
                .AddInvoker<MiraiHttpMessageHandlerInvoker>() // 使用默认的调度器
                .AddClient<MiraiHttpSession>() // 使用默认的客户端
                .Services
                // 由于 MiraiHttpSession 使用 IOptions<MiraiHttpSessionOptions>, 其作为 Singleton 被注册
                // 配置此项将配置基于此 IServiceProvider 全局的连接配置
                // 如果你想一个作用域一个配置的话
                // 自行做一个实现类, 继承MiraiHttpSession, 构造参数中使用 IOptionsSnapshot<MiraiHttpSessionOptions>
                // 并将其传递给父类的构造参数
                // 然后在每一个作用域中!先!配置好 IOptionsSnapshot<MiraiHttpSessionOptions>, 再尝试获取 IMiraiHttpSession
                .Configure<MiraiHttpSessionOptions>(options =>
                {
                    options.Host = "127.0.0.1";
                    options.Port = 8080; // 端口
                    options.AuthKey = opt.AuthKey; // 凭据
                })
                .AddLogging()
                .BuildServiceProvider();
            IServiceScope scope = services.CreateScope();
            await using var x = (IAsyncDisposable)scope;
            //await using AsyncServiceScope scope = services.CreateAsyncScope(); // 自 .NET 6.0 起才可以如此操作代替上边两句
            services = scope.ServiceProvider;
            IMiraiHttpSession session = services.GetRequiredService<IMiraiHttpSession>(); // 大部分服务都基于接口注册, 请使用接口作为类型解析
            //session.AddPlugin(bot); // 实时添加
            bot.SetSession(session);
            await session.ConnectAsync(opt.QQ); // 填入期望连接到的机器人QQ号
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
