using GensouSakuya.QQBot.Core;
using GensouSakuya.GoCqhttp.Sdk.Models.Messages;
using GensouSakuya.GoCqhttp.Sdk.Sessions;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using GensouSakuya.GoCqhttp.Sdk.Sessions.Models.PostEvents.Message;
using GensouSakuya.GoCqhttp.Sdk;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace GensouSakuya.QQBot.Platform.GoCqhttp
{
    public class Bot
    {
        private readonly WebsocketSession _session;
        public WebsocketSession Session => _session;
        public Bot(string host, int port)
        {
            var factory = LoggerFactory.Create(p =>
            {
                p.AddConsole().SetMinimumLevel(LogLevel.Information);
            });
            _session = new WebsocketSession(host, port, null, logger: factory.CreateLogger("bot"));
            _session.PrivateMessageReceived += FriendMessage;
            _session.GroupMessageReceived += GroupMessage;
            _session.GuildMessageReceived += GuildMessage;

            EventCenter.SendMessage += SendMessage;
            EventCenter.GetGroupMemberList += GetGroupMemberList;
            EventCenter.Log += log => { Console.WriteLine(log.Message); };
        }

        public async Task Start()
        {
            await Main.Init();
            await _session.ConnectAsync();
        }

        private async void SendMessage(Core.PlatformModel.Message m)
        {
            var builder = new List<BaseMessage>();
            m.Content.ForEach(p =>
            {
                if (p is Core.PlatformModel.TextMessage tm)
                    builder.Add(new TextMessage(tm.Text));
                else if (p is Core.PlatformModel.ImageMessage im)
                {
                    if (!string.IsNullOrWhiteSpace(im.ImagePath))
                    {
                        //处理成相对路径
                        //或是拷贝到当前目录下
                    }
                    builder.Add(new GensouSakuya.GoCqhttp.Sdk.Models.Messages.ImageMessage
                    {
                        File = im.ImagePath,
                        Url = im.Url,
                    });
                }
                else if (p is Core.PlatformModel.AtMessage am)
                {
                    builder.Add(new AtMessage(am.QQ.ToString()));
                }
                //else if (p is Core.PlatformModel.QuoteMessage qm)
                //{
                //    builder = builder.Add(new Mirai_CSharp.Models.QuoteMessage(qm.QQ));
                //}
                else if (p is Core.PlatformModel.OtherMessage om)
                {
                    if (om.Origin is BaseMessage imb)
                    {
                        builder.Add(imb);
                    }
                }
            });
            if (builder.Count == 0)
                return;
            switch (m.Type)
            {
                case Core.PlatformModel.MessageSourceType.Group:
                    await _session.SendGroupMessage(m.ToGroup.ToString(), builder.ToRawMessage());
                    break;
                case Core.PlatformModel.MessageSourceType.Friend:
                    await _session.SendPrivateMessage(m.ToQQ.ToString(), builder.ToRawMessage());
                    break;
            }
        }

        private List<Core.PlatformModel.BaseMessage> GetMessage(IEnumerable<BaseMessage> chain, out string command)
        {
            command = chain.ToRawMessage();
//            command = string.Join(Environment.NewLine, chain);
            var mes = new List<Core.PlatformModel.BaseMessage>();
            chain.ToList().ForEach(c =>
            {
                if (c is ImageMessage im)
                {
                    mes.Add(new Core.PlatformModel.ImageMessage(url: im.Url, id: null));
                    Console.WriteLine($"received image:{im.Url}");
                }
                else if (c is TextMessage pm)
                {
                    mes.Add(new Core.PlatformModel.TextMessage(pm.Text));
                }
                else if (c is AtMessage am)
                {
                    mes.Add(new Core.PlatformModel.AtMessage(long.TryParse(am.QQ, out var qq) ? qq : default));
                }
                else if (c is ReplyMessage qm)
                {
                    //ignore
                }
                else
                {
                    mes.Add(new Core.PlatformModel.OtherMessage(c));
                }
            });
            return mes;
        }

        private async Task<List<Core.PlatformModel.GroupMemberSourceInfo>> GetGroupMemberList(long groupNo)
        {
            var res = await _session.GetGroupMemberList(groupNo.ToString());
            if (res == null)
                return null;
            return res.Select(p => new Core.PlatformModel.GroupMemberSourceInfo
            {
                Card = p.Card,
                GroupId = groupNo,
                QQId = long.TryParse(p.UserId, out var qq) ? qq : default,
                PermitType = p.Role == "admin" ? Core.PlatformModel.PermitType.Manage :
                    p.Role == "owner" ? Core.PlatformModel.PermitType.Holder : Core.PlatformModel.PermitType.None
            }).ToList();
        }

        public async Task<bool> GroupMessage(object sender, GroupMessage e)
        {
            var message = GetMessage(e.MessageChain, out var command);
            try
            {
                Core.QQManager.UserManager.Add(new Core.PlatformModel.QQSourceInfo
                {
                    Id = long.TryParse(e.UserId, out var qq) ? qq : default,
                    Nick = (string)e.Sender.card,
                });
                await CommandCenter.Execute(command, message, Core.PlatformModel.MessageSourceType.Group, qqNo: e.UserId?.ToLong(),
                    groupNo: e.GroupId?.ToLong());
                return true;
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]{ex.Message}{Environment.NewLine}{ex.StackTrace}[/]");
                return false;
            }
        }

        public async Task<bool> FriendMessage(object sender, PrivateMessage e)
        {
            var message = GetMessage(e.MessageChain, out var command);
            try
            {
                Core.QQManager.UserManager.Add(new Core.PlatformModel.QQSourceInfo
                {
                    Id = long.TryParse(e.UserId, out var qq) ? qq : default,
                    Nick = (string)e.Sender.card,
                });
                await CommandCenter.Execute(command, message, Core.PlatformModel.MessageSourceType.Friend, qqNo: e.UserId?.ToLong());
                return true;
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]{ex.Message}{Environment.NewLine}{ex.StackTrace}[/]");
                return false;
            }
        }

        public async Task<bool> GuildMessage(object sender, GuildMessage e)
        {
            var message = GetMessage(e.MessageChain, out var command);
            try
            {
                //Core.QQManager.UserManager.Add(new Core.PlatformModel.QQSourceInfo
                //{
                //    Id = long.TryParse(e.UserId, out var qq) ? qq : default,
                //    Nick = e.Sender.card,
                //});
                //await CommandCenter.Execute(command, message, Core.PlatformModel.MessageSourceType.Group, qqNo: e.UserId,
                //    groupNo: e.GroupId);
                return true;
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]{ex.Message}{Environment.NewLine}{ex.StackTrace}[/]");
                return false;
            }
        }
    }

    public static class Extensions
    {
        public static long ToLong(this string str)
        {
            return long.TryParse(str, out var num) ? num : default;
        }
    }
}
