using BotSharp.Abstraction.Conversations.Models;
using BotSharp.Abstraction.Functions;
using GensouSakuya.QQBot.Core.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GensouSakuya.QQBot.Core.Agent.Tools
{
    public class GetBilispaceToolFn : IFunctionCallback
    {
        public string Name => "get_bilibili_space";
        public string Indication => "获取B站动态";

        private readonly IServiceProvider _serviceProvider;
        private readonly CacheService _cacheService;

        public GetBilispaceToolFn(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _cacheService = serviceProvider.GetRequiredService<CacheService>();
        }

        public Task<bool> Execute(RoleDialogModel message)
        {
            if(_cacheService.TryGet<List<BiliSpaceDynamic>>(Consts.Cache.BilispaceKey, out var dynamics))
            {
                message.Data = dynamics;
                message.Content = JsonConvert.SerializeObject(dynamics);
            }
            else
            {
                message.Content = "目前没有获取到动态信息";
            }
            return Task.FromResult(true);
        }
    }
}
