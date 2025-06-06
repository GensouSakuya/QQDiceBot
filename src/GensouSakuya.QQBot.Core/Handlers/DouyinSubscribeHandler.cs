using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using GensouSakuya.QQBot.Core.Base;
using GensouSakuya.QQBot.Core.Handlers.Base;
using GensouSakuya.QQBot.Core.Model;
using GensouSakuya.QQBot.Core.PlatformModel;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace GensouSakuya.QQBot.Core.Commands
{
    [Command("douyin")]
    internal class DouyinSubscribeHandler : BaseSubscribeHandler
    {
        private static readonly TimeSpan _subscriberInterval = TimeSpan.FromSeconds(10);
        protected override TimeSpan StartDelay => TimeSpan.FromMinutes(1);
        protected override TimeSpan LoopInterval => TimeSpan.FromMinutes(1);
        private ConcurrentDictionary<string, string> _notFireAgainList = new ConcurrentDictionary<string, string>();
        private const string TemplateUrl = "https://live.douyin.com/webcast/room/web/enter/?aid=6383&app_name=douyin_web&live_id=1&device_platform=web&language=zh-CN&cookie_enabled=true&screen_width=2048&screen_height=1152&browser_language=zh-CN&browser_platform=Win32&browser_name=Edge&browser_version=119.0.0.0&web_rid={0}";

        public DouyinSubscribeHandler(ILoggerFactory loggerFactory, DataManager dataManager) : base(loggerFactory.CreateLogger<DouyinSubscribeHandler>(), dataManager, () => DataManager.Instance.DouyinSubscribers)
        {
        }

        public override async Task Loop(ConcurrentDictionary<string, ConcurrentDictionary<string, SubscribeModel>> subscribers, CancellationToken token)
        {
            try
            {
                using (var client = new RestClient())
                {
                    try
                    {
                        client.AddDefaultHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36 Edg/119.0.0.0");
                        //set cookie
                        await client.GetAsync(new RestRequest("https://live.douyin.com"));
                    }
                    catch (Exception e)
                    {
                        Logger.LogError(e, "douyin refresh cookies error");
                    }
                    foreach (var room in subscribers)
                    {
                        if (room.Value.Count <= 0)
                            continue;

                        try
                        {
                            var url = string.Format(TemplateUrl, room.Key);
                            var res = await client.GetAsync(new RestRequest(url));
                            if (!res.IsSuccessStatusCode)
                            {
                                Logger.LogError(res.ErrorException, "get roominfo failed");
                                continue;
                            }
                            var content = res.Content;
                            var jsonRes = Newtonsoft.Json.JsonConvert.DeserializeObject(content);
                            var jobj = JObject.FromObject(jsonRes);
                            var name = jobj["data"]["user"]["nickname"];
                            var jdata = jobj["data"]["data"];
                            string title = null;
                            var isStreaming = false;
                            StreamType type;
                            if (jdata.HasValues)
                            {
                                var status = jobj["data"]["data"][0]["status"].Value<int>();
                                title = jobj["data"]["data"][0]["title"].Value<string>();
                                if (status == 2)
                                {
                                    isStreaming = true;
                                }
                                type = StreamType.PC;
                            }
                            //else
                            //{
                            //    //拿不到data的时候额外再请求一次，以确保真的是在进行电台直播而非数据异常
                            //    _logger.Info("data is empty, full response:{0}", content);
                            //    var res2 = await client.GetAsync(new RestRequest(url));
                            //    if (!res2.IsSuccessStatusCode)
                            //    {
                            //        _logger.Error(res2.ErrorException, "get roominfo failed");
                            //        continue;
                            //    }
                            //    var content2 = res2.Content;
                            //    var jsonRes2 = Newtonsoft.Json.JsonConvert.DeserializeObject(content2);
                            //    var jobj2 = JObject.FromObject(jsonRes2);
                            //    if (!jobj["data"]["data"].HasValues)
                            //    {
                            //        isStreaming = true;
                            //    }
                            //    type = StreamType.Radio;
                            //}
                            if (isStreaming)
                            {
                                if (_notFireAgainList.ContainsKey(room.Key))
                                    continue;
                                _notFireAgainList.TryAdd(room.Key, room.Key);
                                Logger.LogInformation("douyin[{0}] start sending notice", room.Key);

                                string msg;
                                //if (type == StreamType.Radio)
                                //{
                                //    msg = $"【{name}】开始了电台直播，请使用手机APP观看";
                                //}
                                //else
                                {
                                    msg = $"【{name}】开播了：{title}\nlive点douyin点com/{room.Key}";
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


                                    MessageManager.SendToSource(source, msg);
                                    await Task.Delay(10000);
                                }
                            }
                            else
                            {
                                if (_notFireAgainList.TryRemove(room.Key, out _))
                                {
                                    Logger.LogInformation("douyin[{0}] flag removed", room.Key);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.LogError(e, "douyin failed to send msg");
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
                Logger.LogError(e, "douyin loop error");
            }

        }

        internal enum StreamType
        {
            PC = 0,
            Radio = 1
        }
    }
}
