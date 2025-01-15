using GensouSakuya.QQBot.Core;
using Spectre.Console;
using Mliybs.OneBot.V11;
using Mliybs.OneBot.V11.Data.Receivers.Messages;
using Mliybs.OneBot.V11.Data.Messages;
using Newtonsoft.Json;
using System.Diagnostics;

namespace GensouSakuya.QQBot.Platform.Onebot
{
    public class Bot
    {
        private OneBot _bot;
        private Core.Core _qqbotCore;

        public OneBot OneBot => _bot;

        private IConfiguration _configuration;
        private bool _isRunningInContainer;
        private string? _containerPathPrefix;
        private string? _volumePathPrefix;
        private string? _offlineScript;

        private CancellationTokenSource _heartbeatCancellationTokenSource;

        private readonly static ILoggerFactory _loggerFactory = LoggerFactory.Create(p =>
        {
            p.AddConsole().SetMinimumLevel(LogLevel.Information);
        });
        private readonly ILogger _logger;
        public Bot(string host)
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            Console.WriteLine($"current Env:{env}");
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", true)
                .Build();
            host = host.StartsWith("ws") ? host : $"ws://{host}";
            _logger = _loggerFactory.CreateLogger<Bot>();
            _bot = new OneBot(new WebsocketOneBotHandler(host));
            _bot.MessageReceived.Subscribe(msg =>
            {
                if(msg is GroupMessageReceiver gmr)
                {
                    _logger.LogInformation("收到来自群[{0}]成员[{1}:{2}]的消息:{3}",gmr.GroupId,gmr.Sender.Nickname,gmr.UserId,gmr.RawMessage);
                }
                else if(msg is PrivateMessageReceiver pmr)
                {
                    _logger.LogInformation("收到来自私聊[{0}]的消息:{1}", pmr.UserId, pmr.RawMessage);
                }
            });
            _bot.MessageReceived.AtGroup().Subscribe(async msg => await GroupMessage(msg));
            _bot.MessageReceived.AtPrivate().Subscribe(async msg => await FriendMessage(msg));
            _bot.NoticeReceived.Subscribe(msg =>
            {
                if (msg.NoticeType == "bot_offline")
                {
                    _logger.LogWarning("QQ被踢下线了");
                }
            });

            _heartbeatCancellationTokenSource = new CancellationTokenSource();
            _qqbotCore = new Core.Core(_configuration);
            //_webApplication = LiveChatHelper.Generate(_session).GetAwaiter().GetResult();
        }

        public async Task Start(long qq)
        {
            var dataPath = _configuration["DataPath"];
            _isRunningInContainer = bool.TryParse(_configuration["IsRunningInContainer"], out var result) ? result : _isRunningInContainer;
            if(_isRunningInContainer)
            {
                _containerPathPrefix = _configuration["ContainerPathPrefix"];
                _volumePathPrefix = _configuration["VolumePathPrefix"];
                if (!Directory.Exists(_volumePathPrefix))
                {
                    Directory.CreateDirectory(_volumePathPrefix);
                }
            }
            await _qqbotCore.Init(qq, new Core.PlatformModel.PlatformApiModel
            {
                SendMessage = SendMessage,
                GetGroupMemberList = GetGroupMemberList,
            }, dataPath);
            _offlineScript = _configuration["OfflineRestartScript"];
            if (!string.IsNullOrWhiteSpace(_offlineScript))
            {
                _ = Task.Run(async () => await HeartbeatAsync(_heartbeatCancellationTokenSource.Token));
            }
            //var url = "http://localhost:5202";
            //_webApplication.Run(url);
            //_logger.LogInformation($"弹幕设置页面:{url}/index.html");
        }

        private async Task SendMessage(Core.PlatformModel.Message m)
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
                        if (!string.IsNullOrWhiteSpace(im.ImagePath) && !im.IsTempImg)
                        {
                            //拷贝到当前临时目录下，发送成功后删除
                            var fileName = Path.GetFileName(im.ImagePath);
                            fileName = fileName.Replace("[", "").Replace("]", "");
                            //容器部署的话需要将文件拷贝到挂载目录下，并配置路径为容器目录
                            if (_isRunningInContainer)
                            {
                                var localPath = Path.Combine(_volumePathPrefix, fileName);
                                File.Copy(im.ImagePath, localPath);
                                var containerPath = Path.Combine(_containerPathPrefix, fileName);
                                im.ImagePath = containerPath;
                                deleteFile = localPath;
                            }
                            else
                            {
                                var dir = Path.Combine(Environment.CurrentDirectory, $@"data{Path.PathSeparator}images");
                                if (!Directory.Exists(dir))
                                {
                                    Directory.CreateDirectory(dir);
                                }
                                var newFile = Path.Combine(dir, fileName);
                                File.Copy(im.ImagePath, newFile);
                                im.ImagePath = newFile;
                                deleteFile = newFile;
                            }
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
                await _qqbotCore.HandlerMessage(Core.Model.MessageSource.FromGroup(e.Sender.UserId.ToString(), e.GroupId.ToString(), e.Sender), command, message);
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
                await _qqbotCore.HandlerMessage(Core.Model.MessageSource.FromFriend(e.Sender.UserId.ToString(), e.Sender), command, message);
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

        private async Task HeartbeatAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        var status = await _bot.GetStatus();
                        if (status.Online == false)
                        {
                            var process = new Process
                            {
                                StartInfo = new ProcessStartInfo
                                {
                                    FileName = "sh",
                                    Arguments = $"-c \"{_offlineScript}\""
                                }
                            };
                            process.Start();
                            process.WaitForExit();
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.LogError("heartbeat failed: {0}", e.Message);
                    }
                    finally
                    {
                        await Task.Delay(30000, token);
                    }
                }
            }
            catch(Exception e)
            {
                _logger.LogError(e, "heartbeat error, stopped");
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
