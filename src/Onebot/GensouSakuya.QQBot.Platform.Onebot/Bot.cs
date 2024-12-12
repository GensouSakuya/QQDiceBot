using GensouSakuya.QQBot.Core;
using Spectre.Console;
using Mliybs.OneBot.V11;
using Mliybs.OneBot.V11.Data.Receivers.Messages;
using Mliybs.OneBot.V11.Data.Messages;
using Newtonsoft.Json;

namespace GensouSakuya.QQBot.Platform.Onebot
{
    public class Bot
    {
        private OneBot _bot;

        public OneBot OneBot => _bot;

        private IConfiguration _configuration;

        private readonly static ILoggerFactory _loggerFactory = LoggerFactory.Create(p =>
        {
            p.AddConsole().SetMinimumLevel(LogLevel.Information);
        });
        private readonly ILogger _logger;
        public Bot(string host)
        {
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", true)
                .Build();
            host = host.StartsWith("ws") ? host : $"ws://{host}";
            _logger = _loggerFactory.CreateLogger<Bot>();
            _bot = OneBot.Websocket(host);
            _bot.MessageReceived.Subscribe(msg =>
            {
                if(msg is GroupMessageReceiver gmr)
                {
                    Console.WriteLine($"收到来自群[{gmr.GroupId}]成员[{gmr.Sender.Nickname}:{gmr.UserId}]的消息:{gmr.RawMessage}");
                }
                else if(msg is PrivateMessageReceiver pmr)
                {
                    Console.WriteLine($"收到来自私聊[{pmr.UserId}]的消息:{pmr.RawMessage}");
                }
            });
            _bot.MessageReceived.AtGroup().Subscribe(async msg => await GroupMessage(msg));
            _bot.MessageReceived.AtPrivate().Subscribe(async msg => await FriendMessage(msg));
            EventCenter.SendMessage += SendMessage;
            EventCenter.GetGroupMemberList += GetGroupMemberList;
            EventCenter.Log += log => { Console.WriteLine(log.Message); };

            //_webApplication = LiveChatHelper.Generate(_session).GetAwaiter().GetResult();
        }

        public async Task Start(long qq)
        {
            var dataPath = _configuration["DataPath"];
            await Main.Init(qq, dataPath);
            //var url = "http://localhost:5202";
            //_webApplication.Run(url);
            //_logger.LogInformation($"弹幕设置页面:{url}/index.html");
        }

        private async void SendMessage(Core.PlatformModel.Message m)
        {
            string deleteFile = null;
            try
            {
                if (m.Content.Count == 0)
                    return;
                var builder = new MessageChainBuilder();
                m.Content.ForEach(p =>
                {
                    if (p is Core.PlatformModel.TextMessage tm)
                        builder.Text(tm.Text);
                    else if (p is Core.PlatformModel.ImageMessage im)
                    {
                        if (!string.IsNullOrWhiteSpace(im.ImagePath))
                        {
                            //拷贝到当前临时目录下，发送成功后删除
                            var fileName = Path.GetFileName(im.ImagePath);
                            fileName = fileName.Replace("[", "").Replace("]", "");
                            var newFile = Path.Combine(Environment.CurrentDirectory, @"data\images", fileName);
                            File.Copy(im.ImagePath, newFile);
                            im.ImagePath = fileName;
                            deleteFile = newFile;
                        }
                        else if (!string.IsNullOrWhiteSpace(im.Url))
                        {
                            im.ImagePath = im.Url;
                            im.Url = null;
                        }
                        builder.Image(im.ImagePath);
                    }
                    else if (p is Core.PlatformModel.AtMessage am)
                    {
                        builder.At(am.QQ);
                    }
                    else if( p is Core.PlatformModel.JsonMessage jm)
                    {
                        builder.Json(jm.Json);
                    }
                    //else if (p is Core.PlatformModel.QuoteMessage qm)
                    //{
                    //    builder = builder.Add(new Mirai_CSharp.Models.QuoteMessage(qm.QQ));
                    //}
                    else if (p is Core.PlatformModel.OtherMessage om)
                    {
                        if (om.Origin is MessageBase imb)
                        {
                            builder.Add(imb);
                        }
                    }
                });
                var msg = builder.Build();
                if (msg.Count == 0)
                    return;
                switch (m.Type)
                {
                    case Core.PlatformModel.MessageSourceType.Group:
                        await _bot.SendGroupMessage(m.ToGroup, msg);
                        break;
                    case Core.PlatformModel.MessageSourceType.Friend:
                        await _bot.SendPrivateMessage(m.ToQQ, msg);
                        break;
                    //case Core.PlatformModel.MessageSourceType.Guild:
                    //    await _bot.SendGuildChannelMsg(m.ToGuild, m.ToChannel, builder.ToRawMessage());
                    //    break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "send failed");
            }
            finally
            {
                if (!string.IsNullOrWhiteSpace(deleteFile))
                {
                    try
                    {
                        File.Delete(deleteFile);
                    }
                    catch (Exception e)
                    {
                        //ignore
                    }
                }
            }
        }

        private List<Core.PlatformModel.BaseMessage> GetMessage(MessageReceiver msg, out string command)
        {
            command = msg.RawMessage;
            //            command = string.Join(Environment.NewLine, chain);
            var mes = new List<Core.PlatformModel.BaseMessage>();
            msg.Message.ToList().ForEach(c =>
            {
                if (c is ImageMessage im)
                {
                    mes.Add(new Core.PlatformModel.ImageMessage(url: im.Data.Url));
                    Console.WriteLine($"received image:{im.Data.Url}");
                }
                else if (c is TextMessage pm)
                {
                    mes.Add(new Core.PlatformModel.TextMessage(pm.Data.Text));
                }
                else if (c is AtMessage am)
                {
                    mes.Add(new Core.PlatformModel.AtMessage(long.TryParse(am.Data.QQ, out var qq) ? qq : default));
                }
                else if (c is ReplyMessage qm)
                {
                    //ignore
                }
                else if (c is JsonMessage jm)
                {
                    mes.Add(new Core.PlatformModel.JsonMessage(jm.Data.Data));
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
            var res = await _bot.GetGroupMemberList(groupNo);
            if (res == null)
                return null;
            return res.Select(p => new Core.PlatformModel.GroupMemberSourceInfo
            {
                Card = p.Card,
                GroupId = groupNo,
                QQId = p.UserId,
                PermitType = p.Role ==  Role.Admin ? Core.PlatformModel.PermitType.Manage :
                    p.Role == Role.Owner ? Core.PlatformModel.PermitType.Holder : Core.PlatformModel.PermitType.None
            }).ToList();
        }


        public async Task<bool> GroupMessage(GroupMessageReceiver e)
        {
            var message = GetMessage(e, out var command);
            try
            {
                Core.QQManager.UserManager.Add(new Core.PlatformModel.QQSourceInfo
                {
                    Id = e.Sender.UserId ?? throw new InvalidDataException("group message sender without userid"),
                    Nick = e.Sender.Nickname
                });
                await CommandCenter.Execute(Core.Model.MessageSource.FromGroup(e.Sender.UserId.ToString(), e.GroupId.ToString(), e.Sender), command, message);
                return true;
            }
            catch (Exception ex)
            {
                ProcessErrorMessage($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                return false;
            }
        }

        public async Task<bool> FriendMessage(PrivateMessageReceiver e)
        {
            var message = GetMessage(e, out var command);
            try
            {
                Core.QQManager.UserManager.Add(new Core.PlatformModel.QQSourceInfo
                {
                    Id = e.UserId
                });
                await CommandCenter.Execute(Core.Model.MessageSource.FromFriend(e.Sender.UserId.ToString(), e.Sender), command, message);
                return true;
            }
            catch (Exception ex)
            {
                ProcessErrorMessage($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                return false;
            }
        }

        private static void ProcessErrorMessage(string msg)
        {
            try
            {
                AnsiConsole.MarkupLine($"[red]{msg}[/]");
            }
            catch
            {
                Console.WriteLine(msg);
            }
        }

        private static string ReceiverToString(MessageReceiver msg)
        {
            return JsonConvert.SerializeObject(msg);
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
