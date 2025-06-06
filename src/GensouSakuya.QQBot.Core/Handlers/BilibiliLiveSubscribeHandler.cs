using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GensouSakuya.QQBot.Core.Base;
using GensouSakuya.QQBot.Core.Handlers.Base;
using GensouSakuya.QQBot.Core.Helpers;
using GensouSakuya.QQBot.Core.Model;
using GensouSakuya.QQBot.Core.PlatformModel;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using RestSharp;

namespace GensouSakuya.QQBot.Core.Commands
{
    [Command("bililive")]
    internal class BilibiliLiveSubscribeHandler : BaseSubscribeHandler
    {
        private static readonly TimeSpan _subscriberInterval = TimeSpan.FromSeconds(10);
        protected override TimeSpan StartDelay => TimeSpan.FromMinutes(1);
        protected override TimeSpan LoopInterval => TimeSpan.FromMinutes(1);

        private ConcurrentDictionary<string, string> _notFireAgainList = new ConcurrentDictionary<string, string>();
        private const string ApiTemplateUrl = "https://api.live.bilibili.com/room/v1/Room/room_init?id={0}";

        public BilibiliLiveSubscribeHandler(ILoggerFactory loggerFactory, DataManager dataManager) : base(loggerFactory.CreateLogger<BilibiliLiveSubscribeHandler>(), dataManager, () => dataManager.Config.BiliLiveSubscribers)
        {
        }

        public override async Task Loop(ConcurrentDictionary<string, ConcurrentDictionary<string, SubscribeModel>> subscribers, CancellationToken token)
        {
            try
            {
                using (var client = new RestClient())
                {
                    client.AddDefaultHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36 Edg/119.0.0.0");
                    foreach (var room in subscribers)
                    {
                        if (room.Value.Count <= 0)
                            continue;

                        try
                        {
                            var url = string.Format(ApiTemplateUrl, room.Key);
                            var res = await client.ExecuteAsync(new RestRequest(url, Method.Get));
                            if (!res.IsSuccessStatusCode)
                            {
                                Logger.LogError(res.ErrorException, "get roominfo failed");
                                continue;
                            }
                            var content = res.Content;

                            var jsonRes = Newtonsoft.Json.JsonConvert.DeserializeObject(content);
                            var jobj = JObject.FromObject(jsonRes);
                            var isStreaming = jobj["data"]?["live_status"]?.Value<int>() == 1;
                            if (isStreaming)
                            {
                                if (_notFireAgainList.ContainsKey(room.Key))
                                    continue;
                                _notFireAgainList.TryAdd(room.Key, room.Key);

                                var uid = jobj["data"]["uid"].Value<ulong>();
                                var info = BiliLiveHelper.GetBiliLiveInfo(uid);

                                Logger.LogInformation("bililive[{0}] start sending notice", room.Key);

                                var msgChain = new List<BaseMessage>();
                                msgChain.Add(new TextMessage($"{info.UserName}开播了！"));
                                msgChain.Add(new ImageMessage(url: info.Image));
                                msgChain.Add(new TextMessage($"{info.Title}{Environment.NewLine}https://live.bilibili.com/{room.Key}"));

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


                                    MessageManager.SendToSource(source, msgChain);
                                    await Task.Delay(10000);
                                }
                            }
                            else
                            {
                                if (_notFireAgainList.TryRemove(room.Key, out _))
                                {
                                    Logger.LogInformation("bililive[{0}] flag removed", room.Key);
                                }
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
            }
            catch (Exception e)
            {
                Logger.LogError(e, "bililive loop error");
            }
        }
    }
}
