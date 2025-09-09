using GensouSakuya.QQBot.Agent;
using GensouSakuya.QQBot.Core.Base;
using GensouSakuya.QQBot.Core.Interfaces;
using GensouSakuya.QQBot.Core.Model;
using GensouSakuya.QQBot.Core.PlatformModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GensouSakuya.QQBot.Core.Handlers.Base
{
    internal abstract class BaseAgentChainHandler : IMessageChainHandler
    {
        private readonly IQQBotAgent _agent;
        private readonly IConfiguration _configuration;
        private readonly DataManager _dataManager;
        private readonly string _defaultAgentId;
        private readonly string _commanderName;

        public BaseAgentChainHandler(IServiceProvider serviceProvider, DataManager dataManager, IConfiguration configuration)
        {
            _agent = serviceProvider.GetRequiredService<IQQBotAgent>();
            _dataManager = dataManager;
            _commanderName = GetType().Name;
            _defaultAgentId = GetDefaultAgentId();
            _configuration = configuration;
        }

        public virtual Task<bool> Check(MessageSource source, List<BaseMessage> originMessage, SourceFullInfo sourceInfo)
        {
            if (!_configuration.GetValue<bool>(Consts.Config.EnableAgentKey, false))
                return Task.FromResult(false);
            if (!source.IsPrivateSource)
            {
                var atMessage = originMessage.FirstOrDefault(p => p is AtMessage am && am.QQ == DataManager.QQ);
                if (atMessage == null)
                    return Task.FromResult(false);
            }
            return Task.FromResult(true);
        }

        protected virtual Task<string> GetUserText(MessageSource source, List<BaseMessage> originMessage, SourceFullInfo sourceInfo)
        {
            var message = originMessage.FirstOrDefault(p => p is TextMessage) as TextMessage;
            if (message == null)
                return null;
            return Task.FromResult(message.Text);
        }

        public async Task<bool> NextAsync(MessageSource source, List<BaseMessage> originMessage, SourceFullInfo sourceInfo)
        {
            var agentId = _defaultAgentId;
            var overrideAgentId = _configuration.GetSection(Consts.Config.CommanderAgentMapKey).GetValue<string>(_commanderName, null);
            if (!string.IsNullOrWhiteSpace(overrideAgentId))
                agentId = overrideAgentId;
            
            var text = await GetUserText(source, originMessage, sourceInfo);
            if (string.IsNullOrWhiteSpace(text))
                return true;

            var messages = new List<BaseMessage>();
            if (!source.IsPrivateSource)
            {
                var sourceMessageId = originMessage?.FirstOrDefault()?.Id ?? default;
                messages.Add(new ReplyMessage(sourceInfo.GroupMember.GroupNumber, sourceInfo.GroupMember.QQ, sourceMessageId));
                messages.Add(new AtMessage(sourceInfo.GroupMember.QQ));
            }
            var response = await _agent.ChatOneTimeWithAgent(agentId, text);
            messages.Add(new TextMessage(response));
            MessageManager.SendToSource(source, messages);
            return false;
        }

        private string GetDefaultAgentId()
        {
            var attributes = GetType().GetCustomAttributes(typeof(DefaultAgentAttribute), true);
            if (attributes == null || attributes.Length == 0)
                return null;

            return (attributes.FirstOrDefault() as DefaultAgentAttribute).Id;
        }
    }
}
