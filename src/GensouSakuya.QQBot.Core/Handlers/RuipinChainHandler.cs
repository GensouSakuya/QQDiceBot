using GensouSakuya.QQBot.Agent;
using GensouSakuya.QQBot.Core.Base;
using GensouSakuya.QQBot.Core.Handlers.Base;
using GensouSakuya.QQBot.Core.Model;
using GensouSakuya.QQBot.Core.PlatformModel;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GensouSakuya.QQBot.Core.Handlers
{
    [DefaultAgent("00000000-0000-0000-0000-000000000001")]
    internal class RuipinChainHandler : BaseAgentChainHandler
    {
        public RuipinChainHandler(IServiceProvider serviceProvider, DataManager dataManager, IConfiguration configuration) : base(serviceProvider, dataManager, configuration)
        {
        }

        public override async Task<bool> Check(MessageSource source, List<BaseMessage> originMessage, SourceFullInfo sourceInfo)
        {
            if (!await base.Check(source, originMessage, sourceInfo))
                return false;
            if (!originMessage.Any(p => p is TextMessage tm && tm.Text.Contains("锐评")))
                return false;

            return true;
        }

        //public override async Task NextAsync(MessageSource source, List<BaseMessage> originMessage, UserInfo qq, Group group, GroupMember member, GuildUserInfo guildUser, GuildMember guildmember)
        //{
        //    await Task.Yield();

        //    var sourceMessageId = (originMessage?.FirstOrDefault() as SourceMessage)?.Id ?? default;
        //    var messages = new List<BaseMessage>();
        //    messages.Add(new ReplyMessage(member.GroupNumber, member.QQ, sourceMessageId));
        //    var sentence = GetRandomRuipingSentence();
        //    messages.Add(new TextMessage(sentence));
        //    MessageManager.SendToSource(source, messages);
        //}
    }
}
