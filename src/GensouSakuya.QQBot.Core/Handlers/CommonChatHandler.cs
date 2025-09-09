using GensouSakuya.QQBot.Core.Base;
using GensouSakuya.QQBot.Core.Handlers.Base;
using Microsoft.Extensions.Configuration;
using System;

namespace GensouSakuya.QQBot.Core.Handlers
{
    [ChainOrder(int.MaxValue)]
    [DefaultAgent("00000000-0000-0000-0000-000000000001")]
    internal class CommonChatHandler : BaseAgentChainHandler
    {
        public CommonChatHandler(IServiceProvider serviceProvider, DataManager dataManager, IConfiguration configuration) : base(serviceProvider, dataManager, configuration)
        {
        }
    }
}
