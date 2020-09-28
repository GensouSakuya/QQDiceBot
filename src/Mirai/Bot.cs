using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GensouSakuya.QQBot.Core;
using GensouSakuya.QQBot.Core.PlatformModel;
using GensouSakuya.QQBot.Core.QQManager;
using Mirai_CSharp;
using Mirai_CSharp.Models;
using Mirai_CSharp.Plugin.Interfaces;

namespace GensouSakuya.QQBot.Platform.Mirai
{
    public class Bot : IGroupMessage
    {
        public Bot()
        {
            Main.Init();
            EventCenter.SendMessage += SendMessage;
            EventCenter.GetGroupMemberList += GetGroupMemberList;
            EventCenter.Log += log => { Console.WriteLine(log.Message); };
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
            });
            if (builder.Count == 0)
                return;
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
                }
                else if (c is PlainMessage pm)
                {
                    mes.Add(new TextMessage(pm.Message));
                }
            });
            return mes;
        }

        private async Task<List<GroupMemberSourceInfo>> GetGroupMemberList(long groupNo)
        {
            var res = await _session.GetGroupMemberListAsync(groupNo);
            if (res == null)
                return null;
            return res.Select(p => new GroupMemberSourceInfo
            {
                Card = p.Name,
                GroupId = groupNo,
                QQId = p.Id,
                PermitType = p.Permission == GroupPermission.Administrator ? PermitType.Manage :
                    p.Permission == GroupPermission.Owner ? PermitType.Holder : PermitType.None
            }).ToList();
        }

        private MiraiHttpSession _session;

        public async Task<bool> GroupMessage(MiraiHttpSession session, IGroupMessageEventArgs e)
        {
            _session = session;
            var message = GetMessage(e.Chain,out var command);
            try
            {
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
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Console.ResetColor();
                return false;
            }
        }
    }
}
