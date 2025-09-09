using GensouSakuya.QQBot.Core.Base;
using GensouSakuya.QQBot.Core.Handlers.Base;
using GensouSakuya.QQBot.Core.Model;
using GensouSakuya.QQBot.Core.PlatformModel;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GensouSakuya.QQBot.Core.Handlers
{
    [ChainOrder(1)]
    [DefaultAgent("00000000-0000-0000-0000-000000000002")]
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

        protected override async Task<string> GetUserText(MessageSource source, List<BaseMessage> originMessage, SourceFullInfo sourceInfo)
        {
            var message = originMessage.FirstOrDefault(p => p is TextMessage) as TextMessage;
            if (message == null)
                return null;
            var text = new StringBuilder();
            var replyMessage = originMessage.FirstOrDefault(p => p is ReplyMessage) as ReplyMessage;
            if (replyMessage != null)
            {
                var repliedMsg = await EventCenter.GetMessageById(replyMessage.MessageId);
                if (repliedMsg != null && (repliedMsg.FirstOrDefault(p => p is TextMessage) is TextMessage tx && tx != null))
                    text.AppendLine(tx.Text);
            }
            text.AppendLine(message.Text);
            return text.ToString();
        }
    }
}
