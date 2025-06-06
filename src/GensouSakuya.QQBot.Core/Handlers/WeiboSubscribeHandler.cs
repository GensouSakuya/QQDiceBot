using GensouSakuya.QQBot.Core.Base;
using GensouSakuya.QQBot.Core.Handlers.Base;
using GensouSakuya.QQBot.Core.Helpers;
using GensouSakuya.QQBot.Core.Model;
using GensouSakuya.QQBot.Core.PlatformModel;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GensouSakuya.QQBot.Core.Handlers
{
    [Command("weibo")]
    internal class WeiboSubscribeHandler : BaseSubscribeHandler
    {
        private const string TemplateApiUrl = "https://m.weibo.cn/api/container/getIndex?type=uid&value={0}";
        private readonly ConcurrentDictionary<string, ConcurrentQueue<string>> _lastWeiboId;
        private static readonly TimeSpan _subscriberInterval = TimeSpan.FromSeconds(10);
        protected override TimeSpan StartDelay => TimeSpan.FromSeconds(5);
        protected override TimeSpan LoopInterval => TimeSpan.FromMinutes(10);

        public WeiboSubscribeHandler(ILoggerFactory loggerFactory, DataManager dataManager) : base(loggerFactory.CreateLogger<WeiboSubscribeHandler>(), dataManager, () => dataManager.Config.WeiboSubscribers)
        {
            _lastWeiboId = new ConcurrentDictionary<string, ConcurrentQueue<string>>();
        }

        private string _retryId = null;
        public override async Task Loop(ConcurrentDictionary<string, ConcurrentDictionary<string, SubscribeModel>> subscribers, CancellationToken token)
        {
            using (var client = new RestClient())
            { 
                foreach (var room in subscribers)
                {
                    if (room.Value.Count <= 0)
                        continue;

                    try
                    {
                        var url = string.Format(TemplateApiUrl, room.Key);
                        var res = await client.GetAsync(new RestRequest(url));
                        if (!res.IsSuccessStatusCode)
                        {
                            Logger.LogError(res.ErrorException, "get weibo profile failed");
                            continue;
                        }
                        var content = res.Content;
                        var jobj = JObject.Parse(content);
                        var name = jobj["data"]["userInfo"]["screen_name"];
                        var containerid = jobj["data"]["tabsInfo"]["tabs"][1]["containerid"];
                        res = await client.ExecuteGetAsync(new RestRequest(url + "&containerid=" + containerid));
                        if (!res.IsSuccessStatusCode)
                        {
                            Logger.LogError(res.ErrorException, "get weibo list failed");
                            continue;
                        }
                        content = res.Content;
                        jobj = JObject.Parse(content);
                        var weibos = jobj["data"]["cards"];
                        var isStart = false;
                        var weiboQueue = _lastWeiboId.GetOrAdd(room.Key, p =>
                        {
                            isStart = true;
                            return new ConcurrentQueue<string>();
                        });
                        var targetIndex = -1;
                        var targetWeiboId = "";
                        if (_retryId != null)
                        {
                            var targetId = _retryId;
                            _retryId = null;
                            targetIndex = weibos.ToArray().ToList().FindIndex(0, p => p["mblog"]?["id"]?.ToString() == targetId);
                            if (targetIndex < 0)
                            {
                                continue;
                            }
                            targetWeiboId = targetId;
                        }
                        else
                        {
                            for (var index = 0; index < weibos.Count(); index++)
                            {
                                var weiboId = weibos[index]["mblog"]["id"].ToString();
                                if (isStart)
                                {
                                    weiboQueue.Enqueue(weiboId);
                                    continue;
                                }
                                if (weiboQueue.Contains(weiboId))
                                {
                                    break;
                                }
                                targetIndex = index;
                                targetWeiboId = weiboId;
                            }
                            if (isStart)
                            {
                                continue;
                            }
                        }
                        if (targetIndex < 0)
                        {
                            continue;
                        }
                        weiboQueue.Enqueue(targetWeiboId);
                        if (weiboQueue.Count() > 10)
                            weiboQueue.TryDequeue(out _);
                        var newest = weibos[targetIndex];
                        var id = newest["mblog"]["id"].ToString();
                        var text = newest["mblog"]["text"].ToString();
                        var images = newest["mblog"]["pic_ids"].ToArray();
                        var retweeted = newest["mblog"]["retweeted_status"];

                        Logger.LogDebug("weibo[{0}] start sending notice", room.Key);

                        var isRepost = retweeted != null;
                        text = WeiboHelper.FilterHtml(text);

                        var messages = new List<BaseMessage>();

                        var msg = "";
                        if (!isRepost)
                        {
                            msg = $"{name}发布了微博：{Environment.NewLine}{text}";
                        }
                        else
                        {
                            images = retweeted["pic_ids"].ToArray();
                            var retweetedText = retweeted["text"]?.ToString();
                            retweetedText = WeiboHelper.FilterHtml(retweetedText);
                            msg = $"{name}转发了微博：{Environment.NewLine}{text}{Environment.NewLine}原微博：{Environment.NewLine}@{retweeted["user"]["screen_name"]}：{retweetedText}";
                        }

                        messages.Add(new TextMessage(msg));

                        //List<string> tempImagePaths = null;
                        if (images?.Any() ?? false)
                        {
                            //tempImagePaths = new List<string>();
                            foreach (var image in images)
                            {
                                var imageUrl = $"https://image.baidu.com/search/down?url=https://wx1.sinaimg.cn/large/{image}.jpg";
                                try
                                {
                                    //var savedPath = Path.Combine(DataManager.TempPath, Guid.NewGuid() + ".jpg");
                                    //var imageDownloadRequest = new RestRequest(imageUrl, Method.Get);
                                    //using (var imgStream = await client.DownloadStreamAsync(imageDownloadRequest))
                                    //using (var fileStream = File.OpenWrite(savedPath))
                                    //{
                                    //    await imgStream.CopyToAsync(fileStream);
                                    //}

                                    messages.Add(new ImageMessage(url: imageUrl));
                                    //tempImagePaths.Add(savedPath);
                                }
                                catch (Exception e)
                                {
                                    Logger.LogError(e, "image downloading failed, image:{0}", imageUrl);
                                }
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
                            //if (tempImagePaths != null && tempImagePaths.Any())
                            //{
                            //    foreach (var file in tempImagePaths)
                            //    {
                            //        try
                            //        {
                            //            File.Delete(file);
                            //        }
                            //        catch
                            //        {
                            //            //ignore
                            //        }
                            //    }
                            //}
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.LogError(e, "weibo failed to send msg");
                    }
                    finally
                    {
                        await Task.Delay(_subscriberInterval);
                    }
                }
            }
        }

        public override async Task<bool> ExecuteAsync(MessageSource source, IEnumerable<string> commandArgs, List<BaseMessage> originMessage, SourceFullInfo sourceInfo)
        {
            var res = await base.ExecuteAsync(source, commandArgs, originMessage, sourceInfo);
            if (res)
            {
                if (commandArgs.Count() >= 2 && commandArgs.ElementAt(0) == "retry")
                {
                    _retryId = commandArgs.ElementAt(1);
                }
            }
            return res;
        }
    }
}
