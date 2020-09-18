using System;
using System.Threading.Tasks;
using CommandLine;
using Mirai_CSharp;
using Mirai_CSharp.Models;

namespace GensouSakuya.QQBot.Platform.Mirai
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Options opt = null;
            Parser.Default.ParseArguments<Options>(args).WithParsed<Options>(o =>
            {
                opt = o;
            });

            MiraiHttpSessionOptions options = new MiraiHttpSessionOptions("127.0.0.1", 8080, opt.AuthKey);
            // session 使用 DisposeAsync 模式, 所以使用 await using 自动调用 DisposeAsync 方法。
            // 你也可以不在这里 await using, 不过使用完 session 后请务必调用 DisposeAsync 方法
            await using MiraiHttpSession session = new MiraiHttpSession();
            // 把你实现了 Mirai_CSharp.Plugin.Interfaces 下的接口的类给 new 出来, 然后作为插件塞给 session
            //ExamplePlugin plugin = new ExamplePlugin();
            // 你也可以一个个绑定事件。比如 session.GroupMessageEvt += plugin.GroupMessage;
            // 手动绑定事件后不要再调用AddPlugin, 否则可能导致重复调用
            session.AddPlugin(new Bot());
            // 使用上边提供的信息异步连接到 mirai-api-http
            await session.ConnectAsync(options, opt.QQ); // 自己填机器人QQ号
            while (true)
            {
                if (await Console.In.ReadLineAsync() == "exit")
                {
                    return;
                }
            }
        }
    }
}
