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
            GensouSakuya.QQBot.Core.Main.Init();
            EventCenter.SendMessage += (m) =>
            {
                var builder = new MessageBuilder();
                m.Content.ForEach(async p =>
                {
                    if (p is TextMessage tm)
                        builder.Add(new PlainMessage(tm.Text));
                    else if(p is Core.PlatformModel.ImageMessage im)
                    {
                        //var meg = await _session.up
                    }
                    //builder.Add(new ImageMessage())
                });
                switch (m.Type)
                {
                    case MessageSourceType.Group:
                        //_session.SendGroupMessageAsync(m.ToGroup, new MessageBuilder().Add(new PlainMessage(m.Content)));
                        break;
                }
            };
            EventCenter.Log += log => { Console.WriteLine(log.Message); };
            //EventCenter.GetGroupMemberList += async groupNo =>
            //{
            //    var members = await _session.GetGroupMemberListAsync(groupNo);
            //    members?.ToList().ForEach(member =>
            //    {
            //        var info = new GroupMemberSourceInfo
            //        {
            //            QQId = member.
            //        }
            //    });
            //    return null;
            //};
        }

        private string GetMessage(IMessageBase[] chain)
        {
            return string.Join(Environment.NewLine, ((IEnumerable<IMessageBase>) chain).Skip(1));
        }

        private MiraiHttpSession _session;

        public async Task<bool> GroupMessage(MiraiHttpSession session, IGroupMessageEventArgs e)
        {
            _session = session;
            var message = GetMessage(e.Chain);
            try
            {
                UserManager.Add(new QQSourceInfo
                {
                    Id = e.Sender.Id,
                    Nick = e.Sender.Name,
                });
                await CommandCenter.Execute(message, Core.PlatformModel.MessageSourceType.Group, qqNo: e.Sender.Id,
                    groupNo: e.Sender.Group.Id);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
