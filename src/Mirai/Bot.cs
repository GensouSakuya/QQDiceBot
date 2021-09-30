using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GensouSakuya.QQBot.Core;
using GensouSakuya.QQBot.Core.Exceptions;
using GensouSakuya.QQBot.Core.PlatformModel;
using GensouSakuya.QQBot.Core.QQManager;
using Mirai_CSharp;
using Mirai_CSharp.Exceptions;
using Mirai_CSharp.Models;
using Mirai_CSharp.Plugin.Interfaces;
using Spectre.Console;

namespace GensouSakuya.QQBot.Platform.Mirai
{
    public class Bot : IGroupMessage,IFriendMessage
    {

        private static readonly Logger _logger = Logger.GetLogger<Bot>();

        public Bot()
        {
            EventCenter.SendMessage += SendMessage;
            EventCenter.GetGroupMemberList += GetGroupMemberList;
            EventCenter.Log += log => { Console.WriteLine(log.Message); };
        }

        public async Task Start()
        {
            await Main.Init();
        }

        private async void SendMessage(Message m)
        {
            var builder = (IMessageBuilder)new MessageBuilder();
            m.Content.ForEach(p =>
            {
                if (p is TextMessage tm)
                    builder = builder.Add(new PlainMessage(tm.Text));
                else if (p is Core.PlatformModel.ImageMessage im)
                {
                    if (!string.IsNullOrWhiteSpace(im.ImagePath))
                    {
                        var meg = _session
                            .UploadPictureAsync(
                                m.Type == MessageSourceType.Group ? UploadTarget.Group :
                                m.Type == MessageSourceType.Friend ? UploadTarget.Friend : UploadTarget.Temp, im.ImagePath)
                            .GetAwaiter().GetResult();
                        builder = builder.Add(meg);
                    }
                    else if (!string.IsNullOrWhiteSpace(im.Url))
                    {
                        builder = builder.Add(new Mirai_CSharp.Models.ImageMessage(im.Id, im.Url, im.ImagePath));
                    }
                }
                else if (p is Core.PlatformModel.AtMessage am)
                {
                    builder = builder.Add(new Mirai_CSharp.Models.AtMessage(am.QQ));
                }
                //else if (p is Core.PlatformModel.QuoteMessage qm)
                //{
                //    builder = builder.Add(new Mirai_CSharp.Models.QuoteMessage(qm.QQ));
                //}
                else if(p is Core.PlatformModel.OtherMessage om)
                {
                    if (om.Origin is IMessageBase imb)
                    {
                        builder = builder.Add(imb);
                    }
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
                    await _session.SendGroupMessageAsync(m.ToGroup, builder);
                    break;
                case MessageSourceType.Friend:
                    await _session.SendFriendMessageAsync(m.ToQQ, builder);
                    break;
            }
        }

        private List<BaseMessage> GetMessage(IMessageBase[] chain,out string command)
        {
            command = string.Join(Environment.NewLine, ((IEnumerable<IMessageBase>) chain).Skip(1));
            var mes = new List<BaseMessage>();
            chain.Skip(1).ToList().ForEach(c =>
            {
                if (c is Mirai_CSharp.Models.ImageMessage im)
                {
                    mes.Add(new Core.PlatformModel.ImageMessage(url: im.Url, id: im.ImageId));
                    Console.WriteLine($"received image[{im.ImageId}]:{im.Url}");
                }
                else if (c is PlainMessage pm)
                {
                    mes.Add(new TextMessage(pm.Message));
                }
                else if (c is Mirai_CSharp.Models.AtMessage am)
                {
                    mes.Add(new Core.PlatformModel.AtMessage(am.Target));
                }
                else if (c is Mirai_CSharp.Models.QuoteMessage qm)
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

        private async Task<List<GroupMemberSourceInfo>> GetGroupMemberList(long groupNo)
        {
            if (_session == null)
            {
                _logger.Fatal("_session is null");
                return null;
            }

            try
            {
                var res = await _session.GetGroupMemberListAsync(groupNo);
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

        private MiraiHttpSession _session;

        public async Task<bool> GroupMessage(MiraiHttpSession session, IGroupMessageEventArgs e)
        {
            _session = session;
            var message = GetMessage(e.Chain,out var command);
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
                return true;
            }
            catch(Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]{ex.Message}{Environment.NewLine}{ex.StackTrace}[/]");
                return false;
            }
        }

        public async Task<bool> FriendMessage(MiraiHttpSession session, IFriendMessageEventArgs e)
        {
            _session = session;
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
                return true;
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]{ex.Message}{Environment.NewLine}{ex.StackTrace}[/]");
                return false;
            }
        }
    }
}
