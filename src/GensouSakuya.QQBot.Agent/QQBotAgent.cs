using BotSharp.Abstraction.Conversations.Enums;
using BotSharp.Abstraction.Conversations.Models;
using BotSharp.Abstraction.Conversations;
using Microsoft.Extensions.DependencyInjection;
using BotSharp.Abstraction.Agents.Enums;
using BotSharp.Abstraction.MessageHub.Models;
using BotSharp.Abstraction.MessageHub.Services;
using BotSharp.Abstraction.Routing;

namespace GensouSakuya.QQBot.Agent
{
    public class QQBotAgent : IQQBotAgent
    {
        private readonly IServiceProvider _serviceProvider;

        public QQBotAgent(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<string> ChatWithAgent(string agentId, string text)
        {
            var service = _serviceProvider.GetRequiredService<IConversationService>();
            var conversation = new Conversation
            {
                AgentId = agentId,
                Channel = "qqbot",
                Tags = new(),
            };
            conversation = await service.NewConversation(conversation);
            var conversationId = conversation.Id;
            service.SetConversationId(conversationId, new ());

            var observer = _serviceProvider.GetRequiredService<IObserverService>();
            using var container = observer.SubscribeObservers<HubObserveData<RoleDialogModel>>(conversationId);

            var conv = _serviceProvider.GetRequiredService<IConversationService>();
            var inputMsg = new RoleDialogModel(AgentRole.User, text)
            {
                MessageId = Guid.NewGuid().ToString(),
                CreatedAt = DateTime.UtcNow
            };

            var routing = _serviceProvider.GetRequiredService<IRoutingService>();
            routing.Context.SetMessageId(conversationId, inputMsg.MessageId);

            conv.SetConversationId(conversationId, new ());
            SetStates(conv);

            var responseText = string.Empty;
            await conv.SendMessage(agentId, inputMsg,
                replyMessage: null,
                async msg =>
                {
                    responseText = !string.IsNullOrEmpty(msg.SecondaryContent) ? msg.SecondaryContent : msg.Content;
                });

            return responseText;
        }

        private void SetStates(IConversationService conv)
        {
            if (string.IsNullOrEmpty(conv.States.GetState("channel")))
            {
                conv.States.SetState("channel", string.Empty, source: StateSource.External);
            }
            if (string.IsNullOrEmpty(conv.States.GetState("provider")))
            {
                conv.States.SetState("provider", (string?)null, source: StateSource.External);
            }
            if (string.IsNullOrEmpty(conv.States.GetState("model")))
            {
                conv.States.SetState("model", (string?)null, source: StateSource.External);
            }
            if (string.IsNullOrEmpty(conv.States.GetState("temperature")))
            {
                conv.States.SetState("temperature", 0f, source: StateSource.External);
            }
            if (string.IsNullOrEmpty(conv.States.GetState("sampling_factor")))
            {
                conv.States.SetState("sampling_factor", 0f, source: StateSource.External);
            }
        }
    }
}
