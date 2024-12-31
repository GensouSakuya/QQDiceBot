using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GensouSakuya.QQBot.Core.Base;
using GensouSakuya.QQBot.Core.Handlers.Base;
using GensouSakuya.QQBot.Core.Helpers;
using GensouSakuya.QQBot.Core.Model;
using GensouSakuya.QQBot.Core.PlatformModel;
using Microsoft.Extensions.Logging;
using net.gensousakuya.dice;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using RestSharp;
using static GensouSakuya.QQBot.Core.PlatformManager;

namespace GensouSakuya.QQBot.Core.Handlers
{
    [Command("bilispace")]
    internal class BilibiliSpaceSubscribeHandler : BaseSubscribeHandler
    {
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly ConcurrentDictionary<string, string> _notFireAgainList;
        private readonly ConcurrentDictionary<string, ConcurrentQueue<string>> _lastDynamicId;
        private static readonly TimeSpan _subscriberInterval = TimeSpan.FromSeconds(10);
        protected override TimeSpan StartDelay => TimeSpan.FromMinutes(1);
        protected override TimeSpan LoopInterval => TimeSpan.FromHours(1);

        public BilibiliSpaceSubscribeHandler(ILoggerFactory loggerFactory) : base(loggerFactory.CreateLogger<BilibiliSpaceSubscribeHandler>(), () => DataManager.Instance.BiliSpaceSubscribers)
        {
            _notFireAgainList = new ConcurrentDictionary<string, string>();
            _cancellationTokenSource = new CancellationTokenSource();
            _lastDynamicId = new ConcurrentDictionary<string, ConcurrentQueue<string>>();
        }

        protected override async Task Loop(ConcurrentDictionary<string, ConcurrentDictionary<string, SubscribeModel>> subscribers, CancellationToken token)
        {
            foreach (var room in subscribers)
            {
                if (room.Value.Count <= 0)
                    continue;

                try
                {
                    var dynamics = await BiliLiveHelper.GetBiliSpaceDynm(room.Key);
                    dynamics = dynamics.Where(p => !p.IsTop).ToList();

                    var isStart = false;
                    var dynamicQueue = _lastDynamicId.GetOrAdd(room.Key, p => {
                        isStart = true;
                        return new ConcurrentQueue<string>();
                    });
                    var targetIndex = -1;
                    var targetDynId = "";
                    for (var index = 0; index < dynamics.Count; index++)
                    {
                        var dynId = dynamics[index].Id;
                        if (isStart)
                        {
                            dynamicQueue.Enqueue(dynId);
                            continue;
                        }
                        if (dynamicQueue.Contains(dynId))
                        {
                            break;
                        }
                        targetIndex = index;
                        targetDynId = dynId;
                    }
                    if (isStart)
                    {
                        continue;
                    }
                    if (targetIndex < 0)
                    {
                        continue;
                    }
                    dynamicQueue.Enqueue(targetDynId);
                    if (dynamicQueue.Count > 20)
                        dynamicQueue.TryDequeue(out _);
                    var newest = dynamics[targetIndex];
                    var id = newest.Id;
                    var text = newest.Content;
                    var images = newest.Images;

                    Logger.LogInformation("Bili dynamic [{0}] start sending notice", room.Key);

                    var isRepost = newest.IsRepost;
                    var messages = new List<BaseMessage>();

                    var msg = "";
                    if (!isRepost)
                    {
                        msg = $"{newest.AuthorName}发布了B站动态：{Environment.NewLine}{text}";
                    }
                    else
                    {
                        var retweeted = newest.RepostOrigin;
                        msg = $"{newest.AuthorName}转发了B站动态：{Environment.NewLine}{text}{Environment.NewLine}原动态：{Environment.NewLine}@{retweeted.AuthorName}：{retweeted.Content}";
                    }

                    messages.Add(new TextMessage(msg));

                    if (images?.Any() ?? false)
                    {
                        foreach (var image in images)
                        {
                            messages.Add(new ImageMessage(url: image));
                        }
                    }

                    foreach (var sor in room.Value)
                    {
                        MessageSource source;
                        var sorModel = sor.Value;
                        if (sorModel.Source == MessageSourceType.Group)
                            source = MessageSource.FromGroup(null, sorModel.SourceId, null);
                        else if (sorModel.Source == MessageSourceType.Guild)
                        {
                            var ids = sorModel.SourceId.Split('+');
                            if (ids.Length < 2)
                                continue;

                            source = MessageSource.FromGuild(null, ids[0], ids[1], null);
                        }
                        else if (sorModel.Source == MessageSourceType.Friend)
                        {
                            source = MessageSource.FromFriend(sorModel.SourceId, null);
                        }
                        else
                            continue;


                        MessageManager.SendToSource(source, messages);
                        await Task.Delay(10000);
                    }
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "bililive failed to send msg");
                }
                finally
                {
                    await Task.Delay(_subscriberInterval);
                }
            }
        }

        public override async Task ExecuteAsync(MessageSource source, IEnumerable<string> commandArgs, List<BaseMessage> originMessage, SourceFullInfo sourceInfo)
        {
            await base.ExecuteAsync(source, commandArgs, originMessage, sourceInfo);
        }
    }
}
