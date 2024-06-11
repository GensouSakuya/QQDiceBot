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
using System.IO;
using Microsoft.AspNetCore.Builder;

namespace GensouSakuya.QQBot.Platform.GoCqhttp
{
    public class Bot
    {
        private readonly WebsocketSession _session;
        public WebsocketSession Session => _session;

        private WebApplication _webApplication;
        private readonly static ILoggerFactory _loggerFactory = LoggerFactory.Create(p =>
        {
            p.AddConsole().SetMinimumLevel(LogLevel.Information);
        });
        private readonly ILogger _logger;
        public Bot(string host, int port)
        {
            _logger = _loggerFactory.CreateLogger<Bot>();
            _session = new WebsocketSession(host, port, null, logger: _logger);
            _session.PrivateMessageReceived += FriendMessage;
            _session.GroupMessageReceived += GroupMessage;
            _session.GuildMessageReceived += GuildMessage;

            EventCenter.SendMessage += SendMessage;
            EventCenter.GetGroupMemberList += GetGroupMemberList;
            EventCenter.GetGuildMember += GetGuildMember;
            EventCenter.Log += log => { Console.WriteLine(log.Message); };

            //_webApplication = LiveChatHelper.Generate(_session).GetAwaiter().GetResult();
        }

        public async Task Start(string qq)
        {
            await Main.Init(qq.ToLong());
            await _session.ConnectAsync();
            //var url = "http://localhost:5202";
            //_webApplication.Run(url);
            //_logger.LogInformation($"弹幕设置页面:{url}/index.html");
        }

        private async void SendMessage(Core.PlatformModel.Message m)
        {
            string deleteFile = null;
            try
            {
                ;
                var builder = new List<BaseMessage>();
                m.Content.ForEach(p =>
                {
                    if (p is Core.PlatformModel.TextMessage tm)
                        builder.Add(new TextMessage(tm.Text));
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
                        builder.Add(new ImageMessage(im.ImagePath, 0, im.Url));
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
                    case Core.PlatformModel.MessageSourceType.Guild:
                        await _session.SendGuildChannelMsg(m.ToGuild, m.ToChannel, builder.ToRawMessage());
                        break;
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

        private List<Core.PlatformModel.BaseMessage> GetMessage(IEnumerable<BaseMessage> chain, out string command)
        {
            command = chain.ToRawMessage();
            //            command = string.Join(Environment.NewLine, chain);
            var mes = new List<Core.PlatformModel.BaseMessage>();
            chain.ToList().ForEach(c =>
            {
                if (c is ImageMessage im)
                {
                    mes.Add(new Core.PlatformModel.ImageMessage(url: im.Url));
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
                else if(c is JsonMessage jm)
                {
                    mes.Add(new Core.PlatformModel.JsonMessage(jm.Data));
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

        private async Task<Core.PlatformModel.GuildMemberSourceInfo> GetGuildMember(string userId, string guildId)
        {
            try
            {
                var res = await _session.GetGuildMemberProfile(guildId, userId);
                if (res == null)
                    return null;
                return new Core.PlatformModel.GuildMemberSourceInfo
                {
                    GuildId = guildId,
                    UserId = res.TinyId,
                    NickName = res.NickName
                };
            }
            catch (Exception ex)
            {
                if (ex.Message?.Contains("58002") ?? false)
                {
                    var list = await _session.GetGuildMemberList(guildId, null);
                    if (list?.Members != null)
                    {
                        var user = list.Members.FirstOrDefault(p => p.TinyId == userId);
                        if (user != null)
                        {
                            return new Core.PlatformModel.GuildMemberSourceInfo
                            {
                                GuildId = guildId,
                                UserId = user.TinyId,
                                NickName = user.NickName
                            };
                        }
                    }
                }
                return null;
            }
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
                await CommandCenter.Execute(Core.Model.MessageSource.FromGroup(e.UserId, e.GroupId, e.Sender), command, message);
                return true;
            }
            catch (Exception ex)
            {
                ProcessErrorMessage($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
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
                await CommandCenter.Execute(Core.Model.MessageSource.FromFriend(e.UserId, e.Sender), command, message);
                return true;
            }
            catch (Exception ex)
            {
                ProcessErrorMessage($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                return false;
            }
        }

        public async Task<bool> GuildMessage(object sender, GuildMessage e)
        {
            var message = GetMessage(e.MessageChain, out var command);
            try
            {
                Core.QQManager.GuildUserManager.Add(new Core.Model.GuildUserInfo
                {
                    Id = e.UserId,
                });
                await CommandCenter.Execute(Core.Model.MessageSource.FromGuild(e.UserId, e.GuildId, e.ChannelId, e.Sender), command, message);
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
    }

    public static class Extensions
    {
        public static long ToLong(this string str)
        {
            return long.TryParse(str, out var num) ? num : default;
        }
    }
}
