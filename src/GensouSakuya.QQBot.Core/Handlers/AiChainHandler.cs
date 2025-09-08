using GensouSakuya.QQBot.Core.Base;
using GensouSakuya.QQBot.Core.Helpers;
using GensouSakuya.QQBot.Core.Interfaces;
using GensouSakuya.QQBot.Core.Model;
using GensouSakuya.QQBot.Core.PlatformModel;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GensouSakuya.QQBot.Core.Handlers
{
    internal class AiChainHandler : IMessageChainHandler
    {
        private DataManager _dataManager;
        private IAiService _aiService;

        public AiChainHandler(DataManager dataManager, AiServiceFactory aiServiceFactory) 
        {
            _dataManager = dataManager;
            _aiService = aiServiceFactory.Acquire();
        }

        public Task<bool> Check(MessageSource source, List<BaseMessage> originMessage, SourceFullInfo sourceInfo)
        {
            if (originMessage == null)
                return Task.FromResult(false);
            var aiConfig = _dataManager?.Config.AiConfig;
            if (aiConfig == null)
                return Task.FromResult(false);
            if (string.IsNullOrWhiteSpace(aiConfig.APIKey))
                return Task.FromResult(false);
            if (source.Type != MessageSourceType.Group && source.Type != MessageSourceType.Discuss && source.Type != MessageSourceType.Friend)
                return Task.FromResult(false);
            if (source.Type == MessageSourceType.Group && (!_dataManager.Config.AiEnableConifig.TryGetValue(source.GroupIdNum.Value ?? -1, out var res) || !res))
                return Task.FromResult(false);
            if (!originMessage.Any(p => p is TextMessage))
                return Task.FromResult(false);
            if(source.Type == MessageSourceType.Group)
            {
                var atMessage = originMessage.FirstOrDefault(p => p is AtMessage am && am.QQ == DataManager.QQ);
                if (atMessage == null)
                    return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        public async Task<bool> NextAsync(MessageSource source, List<BaseMessage> originMessage, SourceFullInfo sourceInfo)
        {
            var sourceText = (originMessage.FirstOrDefault(p => p is TextMessage) as TextMessage)?.Text;
            if (string.IsNullOrWhiteSpace(sourceText)) 
                return true;

            var messages = new List<BaseMessage>();
            var sourceMessageId = originMessage?.FirstOrDefault()?.Id ?? default;
            if (source.Type == MessageSourceType.Group)
            {
                var member = sourceInfo.GroupMember;
                messages.Add(new ReplyMessage(member.GroupNumber, member.QQ, sourceMessageId));
                messages.Add(new AtMessage(member.QQ));
            }
            else
            {
                messages.Add(new ReplyMessage(null, null, sourceMessageId));
            }

            //if (limit?.Check(config.TokenLimit) == false)
            //{
            //    messages.Add(new TextMessage(config.TokenNotEnoughMessage));
            //    MessageManager.SendToSource(source, messages);
            //    return false;
            //}

            var reply = await _aiService.Chat(sourceText);
            if(string.IsNullOrWhiteSpace(reply)) 
                return true;

            messages.Add(new TextMessage(reply));
            MessageManager.SendToSource(source, messages);
            return false;
        }
    }

    public class AiConfig
    {
        public AiType Type { get; set; }
        public string APIKey { get; set; } = "";
        public long TokenLimit { get; set; } = 500000;
        public string TokenNotEnoughMessage { get; set; } = "Token额度不够啦！";
        public string SystemPromptFilePath { get; set; }
        public string UserPrompFilePath { get; set; }
        public JToken Spec { get; set; }
    }

    public enum AiType
    {
        None = 0,
        QWen = 1,
        Deepseek = 2,
    }
}
