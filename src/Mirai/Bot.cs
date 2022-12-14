using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GensouSakuya.QQBot.Core;
using GensouSakuya.QQBot.Core.Exceptions;
using GensouSakuya.QQBot.Core.PlatformModel;
using GensouSakuya.QQBot.Core.QQManager;
using Mirai.CSharp.Exceptions;
using Mirai.CSharp.HttpApi.Handlers;
using Mirai.CSharp.HttpApi.Models.ChatMessages;
using Mirai.CSharp.HttpApi.Models.EventArgs;
using Mirai.CSharp.HttpApi.Parsers;
using Mirai.CSharp.HttpApi.Parsers.Attributes;
using Mirai.CSharp.HttpApi.Session;
using Mirai.CSharp.Models;
using Spectre.Console;
using AtMessage = Mirai.CSharp.HttpApi.Models.ChatMessages.AtMessage;
using ImageMessage = Mirai.CSharp.HttpApi.Models.ChatMessages.ImageMessage;
using QuoteMessage = Mirai.CSharp.HttpApi.Models.ChatMessages.QuoteMessage;
using SourceMessage = Mirai.CSharp.HttpApi.Models.ChatMessages.SourceMessage;
using VoiceMessage = Mirai.CSharp.HttpApi.Models.ChatMessages.VoiceMessage;

namespace GensouSakuya.QQBot.Platform.Mirai
{
    [RegisterMiraiHttpParser(typeof(DefaultMappableMiraiHttpMessageParser<IGroupMessageEventArgs, GroupMessageEventArgs>))]
    [RegisterMiraiHttpParser(typeof(DefaultMappableMiraiHttpMessageParser<IFriendMessageEventArgs, FriendMessageEventArgs>))]
    public class Bot : IMiraiHttpMessageHandler<IGroupMessageEventArgs> , IMiraiHttpMessageHandler<IFriendMessageEventArgs>, IMiraiHttpMessageHandler<IDisconnectedEventArgs>
    {

        private static readonly Logger _logger = Logger.GetLogger<Bot>();

        public Bot()
        {
            EventCenter.SendMessage += SendMessage;
            EventCenter.GetGroupMemberList += GetGroupMemberList;
            EventCenter.Log += log => { Console.WriteLine(log.Message); };
        }

        public async Task Start(long qq)
        {
            await Main.Init(qq);
        }

        private async void SendMessage(Message m)
        {
            try
            {
                var session = GetAndValidateSession();
                if (session == null)
                    return;
                var builder = new List<IChatMessage>();
                int? quotaMessageId = null;
                m.Content.ForEach(p =>
                {
                    if (p is TextMessage tm)
                        builder.Add(new PlainMessage(tm.Text));
                    else if (p is Core.PlatformModel.ImageMessage im)
                    {
                        if (!string.IsNullOrWhiteSpace(im.ImagePath))
                        {
                            var meg = session
                                .UploadPictureAsync(
                                    m.Type == MessageSourceType.Group ? UploadTarget.Group :
                                    m.Type == MessageSourceType.Friend ? UploadTarget.Friend : UploadTarget.Temp,
                                    im.ImagePath)
                                .GetAwaiter().GetResult();
                            builder.Add((IChatMessage)meg);
                        }
                        else if (!string.IsNullOrWhiteSpace(im.Url))
                        {
                            builder.Add(new ImageMessage(im.ImageId, im.Url, im.ImagePath));
                        }
                    }
                    else if (p is Core.PlatformModel.AtMessage am)
                    {
                        builder.Add(new AtMessage(am.QQ));
                    }
                    else if (p is Core.PlatformModel.QuoteMessage qm)
                    {
                        quotaMessageId = (int)qm.MessageId;
                    }
                    else if (p is Core.PlatformModel.OtherMessage om)
                    {
                        if (om.Origin is IChatMessage imb)
                        {
                            builder.Add(imb);
                        }
                    }
                    else if (p is Core.PlatformModel.SourceMessage)
                    {
                        //ignore
                    }
                });
                if (builder.Count == 0)
                    return;
                if (_session == null)
                {
                    _logger.Fatal("_session is null");
                    return;
                }

                switch (m.Type)
                {
                    case MessageSourceType.Group:
                        await _session.SendGroupMessageAsync(m.ToGroup, builder.ToArray(), quotaMessageId);
                        break;
                    case MessageSourceType.Friend:
                        await _session.SendFriendMessageAsync(m.ToQQ, builder.ToArray(), quotaMessageId);
                        break;
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "send message error");
            }
        }

        private List<BaseMessage> GetMessage(IChatMessage[] chain,out string command)
        {
            command = string.Join(Environment.NewLine, ((IEnumerable<IChatMessage>) chain).Skip(1));
            var mes = new List<BaseMessage>();
            chain.ToList().ForEach(c =>
            {
                if (c is SourceMessage sm)
                {
                    mes.Add(new Core.PlatformModel.SourceMessage(sm.Id, sm.Time));
                }
                else if (c is ImageMessage im)
                {
                    mes.Add(new Core.PlatformModel.ImageMessage(url: im.Url, imageId: im.ImageId));
                    Console.WriteLine($"received image[{im.ImageId}]:{im.Url}");
                }
                else if (c is VoiceMessage vm)
                {
                    mes.Add(new Core.PlatformModel.VoiceMessage(url: vm.Url, voiceId: vm.VoiceId));
                    Console.Write($"received voice[{vm.VoiceId}]:{vm.Url}");
                }
                else if (c is PlainMessage pm)
                {
                    mes.Add(new TextMessage(pm.Message));
                }
                else if (c is AtMessage am)
                {
                    mes.Add(new Core.PlatformModel.AtMessage(am.Target));
                }
                else if (c is QuoteMessage qm)
                {
                    mes.Add(new Core.PlatformModel.QuoteMessage(qm.GroupId, qm.SenderId, qm.Id));
                }
                else
                {
                    mes.Add(new Core.PlatformModel.OtherMessage(c));
                }
            });
            return mes;
        }

        private async Task<List<GroupMemberSourceInfo>> GetGroupMemberList(long groupNo)
        {
            var session = GetAndValidateSession();
            if (session == null)
                return null;

            try
            {
                var res = await session.GetGroupMemberListAsync(groupNo);
                return res.Select(p => new GroupMemberSourceInfo
                {
                    Card = p.Name,
                    GroupId = groupNo,
                    QQId = p.Id,
                    PermitType = p.Permission == GroupPermission.Administrator ? PermitType.Manage :
                        p.Permission == GroupPermission.Owner ? PermitType.Holder : PermitType.None
                }).ToList();
            }
            catch (TargetNotFoundException)
            {
                throw new GroupNotExistsException(groupNo);
            }
        }

        private IMiraiHttpSession _session;

        public void SetSession(IMiraiHttpSession session)
        {
            _session = session;
        }

        private IMiraiHttpSession GetAndValidateSession()
        {
            if(_session == null)
            {
                _logger.Fatal("_session is null");
                return null;
            }

            if (!_session.Connected)
            {
                _logger.Fatal("_session is disconnected");
                return null;
            }

            return _session;
        }

        public async Task HandleMessageAsync(IMiraiHttpSession client, IGroupMessageEventArgs e)
        {
            _session = client;
            var message = GetMessage(e.Chain, out var command);
            try
            {
                //mirai不支持获取单个用户信息，所以在消息收到时进行添加
                UserManager.Add(new QQSourceInfo
                {
                    Id = e.Sender.Id,
                    Nick = e.Sender.Name,
                });
                await CommandCenter.Execute(command, message, Core.PlatformModel.MessageSourceType.Group, qqNo: e.Sender.Id,
                    groupNo: e.Sender.Group.Id);
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]{ex.Message}{Environment.NewLine}{ex.StackTrace}[/]");
            }
        }

        public async Task HandleMessageAsync(IMiraiHttpSession client, IFriendMessageEventArgs e)
        {
            _session = client;
            var message = GetMessage(e.Chain, out var command);
            try
            {
                //mirai不支持获取单个用户信息，所以在消息收到时进行添加
                UserManager.Add(new QQSourceInfo
                {
                    Id = e.Sender.Id,
                    Nick = e.Sender.Name,
                });
                await CommandCenter.Execute(command, message, Core.PlatformModel.MessageSourceType.Friend, qqNo: e.Sender.Id);
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]{ex.Message}{Environment.NewLine}{ex.StackTrace}[/]");
            }
        }

        public async Task HandleMessageAsync(IMiraiHttpSession client, IDisconnectedEventArgs message)
        {
            _logger.Error("disconnected, reconnecting");
            await client.ConnectAsync(client.QQNumber.Value);
        }
    }
}
