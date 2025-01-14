//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.IO;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;
//using GensouSakuya.QQBot.Core.Base;
//using GensouSakuya.QQBot.Core.Helpers;
//using GensouSakuya.QQBot.Core.Model;
//using GensouSakuya.QQBot.Core.PlatformModel;
//using net.gensousakuya.dice;
//using Newtonsoft.Json.Linq;
//using RestSharp;

//namespace GensouSakuya.QQBot.Core.Commands
//{
//    [Command("weibo")]
//    public class WeiboSubscribeManager : BaseManager
//    {
//        private static readonly  Logger _logger = Logger.GetLogger<WeiboSubscribeManager>();

//        public override async System.Threading.Tasks.Task ExecuteAsync(MessageSource source, List<string> command, List<BaseMessage> originMessage, UserInfo qq, Group group, GroupMember member, GuildUserInfo guildUser, GuildMember guildmember)
//        {
//            SubscribeModel sbm;
//            if (source.Type == MessageSourceType.Group)
//            {
//                if (member.QQ != DataManager.Instance.AdminQQ)
//                {
//                    MessageManager.SendToSource(source, "目前只有机器人管理员可以配置该功能哦");
//                    return;
//                }

//                sbm = new SubscribeModel
//                {
//                    Source = MessageSourceType.Group,
//                    SourceId = source.GroupId
//                };
//            }
//            else if(source.Type == MessageSourceType.Guild)
//            {
//                if (guildmember.UserId != DataManager.Instance.AdminGuildUserId)
//                {
//                    MessageManager.SendToSource(source, "目前只有机器人管理员可以配置该功能哦");
//                    return;
//                }

//                sbm = new SubscribeModel
//                {
//                    Source = MessageSourceType.Guild,
//                    SourceId = $"{source.GuildId}+{source.ChannelId}"
//                };
//            }
//            else if(source.Type == MessageSourceType.Friend)
//            {
//                if (source.QQ != DataManager.Instance.AdminQQ.ToString())
//                {
//                    MessageManager.SendToSource(source, "目前只有机器人管理员可以配置该功能哦");
//                    return;
//                }

//                sbm = new SubscribeModel
//                {
//                    Source = MessageSourceType.Friend,
//                    SourceId = source.QQ
//                };
//            }
//            else
//            {
//                MessageManager.SendToSource(source, "懒得支持！");
//                return;
//            }

//            if (command.Count < 1)
//            {
//                return;
//            }

//            var first = command[0];
//            if (first == "trigger")
//            {
//                _completionSource.TrySetResult(true);
//            }
//            else
//            {
//                if (command.Count < 2)
//                {
//                    return;
//                }
//                var roomId = command[1];

//                if (first == "subscribe")
//                {
//                    var sub = Subscribers.GetOrAdd(roomId, new ConcurrentDictionary<string, SubscribeModel>());
//                    if (sub.ContainsKey(sbm.ToString()))
//                    {
//                        MessageManager.SendToSource(source, "该微博已订阅");
//                        return;
//                    }

//                    sub[sbm.ToString()] = sbm;
//                    MessageManager.SendToSource(source, "订阅成功！");
//                    DataManager.Instance.NoticeConfigUpdated();
//                    return;
//                }
//                else if (first == "unsubscribe")
//                {
//                    if (!Subscribers.TryGetValue(roomId, out var sub))
//                    {
//                        return;
//                    }
//                    if (sub.Remove(sbm.ToString(), out _))
//                    {
//                        MessageManager.SendToSource(source, "取消订阅成功！");
//                        DataManager.Instance.NoticeConfigUpdated();
//                    }
//                    return;
//                }
//            }

//            return;
//        }
        
//        private static ConcurrentDictionary<string, ConcurrentDictionary<string, SubscribeModel>> _subscribers { get; set; }
//        public static ConcurrentDictionary<string, ConcurrentDictionary<string, SubscribeModel>> Subscribers
//        {
//            get => _subscribers;
//            set
//            {
//                if (value == null)
//                {
//                    _subscribers = new ConcurrentDictionary<string, ConcurrentDictionary<string, SubscribeModel>>();
//                }
//                else
//                {
//                    _subscribers = value;
//                }
//            }
//        }

//        static CancellationTokenSource _cancellationTokenSource;
//        static WeiboSubscribeManager()
//        {
//            _lastWeiboId = new ConcurrentDictionary<string, ConcurrentQueue<string>>();
//            _cancellationTokenSource = new CancellationTokenSource();
//            Task.Run(() => LoopCheck(_cancellationTokenSource.Token));
//        }

//        private static TaskCompletionSource<bool> _completionSource = new TaskCompletionSource<bool>();

//        private static ConcurrentDictionary<string, ConcurrentQueue<string>> _lastWeiboId;
//        private static async Task LoopCheck(CancellationToken token)
//        {
//            try
//            {
//                await Task.WhenAny(Task.Delay(5000), _completionSource.Task);
//                var loopSpan = new TimeSpan(0, 10, 0);
//                var intervalSpan = new TimeSpan(0, 0, 10);
//                var templateUrl = "https://m.weibo.cn/api/container/getIndex?type=uid&value={0}";
//                while (!token.IsCancellationRequested)
//                {
//                    if (_completionSource.Task.IsCompleted)
//                        _completionSource = new TaskCompletionSource<bool>();
//                    using (var client = new RestClient())
//                    {
//                        foreach (var room in Subscribers)
//                        {
//                            if (room.Value.Count <= 0)
//                                continue;

//                            try
//                            {
//                                var url = string.Format(templateUrl, room.Key);
//                                var res = await client.GetAsync(new RestRequest(url));
//                                if (!res.IsSuccessStatusCode)
//                                {
//                                    _logger.Error(res.ErrorException, "get weibo profile failed");
//                                    continue;
//                                }
//                                var content = res.Content;
//                                var jobj = JObject.Parse(content);
//                                var name = jobj["data"]["userInfo"]["screen_name"];
//                                var containerid = jobj["data"]["tabsInfo"]["tabs"][1]["containerid"];
//                                res = await client.ExecuteGetAsync(new RestRequest(url + "&containerid=" + containerid));
//                                if (!res.IsSuccessStatusCode)
//                                {
//                                    _logger.Error(res.ErrorException, "get weibo list failed");
//                                    continue;
//                                }
//                                content = res.Content;
//                                jobj = JObject.Parse(content);
//                                var weibos = jobj["data"]["cards"];
//                                var isStart = false;
//                                var weiboQueue = _lastWeiboId.GetOrAdd(room.Key, p => {
//                                    isStart = true;
//                                    return new ConcurrentQueue<string>();
//                                });
//                                var targetIndex = -1;
//                                var targetWeiboId = "";
//                                for (var index = 0; index < weibos.Count(); index++)
//                                {
//                                    var weiboId = weibos[index]["mblog"]["id"].ToString();
//                                    if (isStart)
//                                    {
//                                        weiboQueue.Enqueue(weiboId);
//                                        continue;
//                                    }
//                                    if (weiboQueue.Contains(weiboId))
//                                    {
//                                        break;
//                                    }
//                                    targetIndex = index;
//                                    targetWeiboId = weiboId;
//                                }
//                                if (isStart)
//                                {
//                                    continue;
//                                }
//                                if (targetIndex < 0)
//                                {
//                                    continue;
//                                }
//                                weiboQueue.Enqueue(targetWeiboId);
//                                if (weiboQueue.Count > 10)
//                                    weiboQueue.TryDequeue(out _);
//                                var newest = weibos[targetIndex];
//                                var id = newest["mblog"]["id"].ToString();
//                                var text = newest["mblog"]["text"].ToString();
//                                var images = newest["mblog"]["pic_ids"].ToArray();
//                                var retweeted = newest["mblog"]["retweeted_status"];

//                                _logger.Info("weibo[{0}] start sending notice", room.Key);

//                                var isRepost = retweeted != null;
//                                text = WeiboHelper.FilterHtml(text);

//                                var messages = new List<BaseMessage>();
                                
//                                var msg = "";
//                                if (!isRepost)
//                                {
//                                    msg = $"{name}发布了微博：{Environment.NewLine}{text}";
//                                }
//                                else
//                                {
//                                    images = retweeted["pic_ids"].ToArray();
//                                    var retweetedText = retweeted["text"]?.ToString();
//                                    retweetedText = WeiboHelper.FilterHtml(retweetedText);
//                                    msg = $"{name}转发了微博：{Environment.NewLine}{text}{Environment.NewLine}原微博：{Environment.NewLine}@{retweeted["user"]["screen_name"]}：{retweetedText}";
//                                }

//                                messages.Add(new TextMessage(msg));

//                                List<string> tempImagePaths = null;
//                                if (images?.Any() ?? false)
//                                {
//                                    tempImagePaths = new List<string>();
//                                    foreach (var image in images)
//                                    {
//                                        var imageUrl = $"https://image.baidu.com/search/down?url=https://wx1.sinaimg.cn/large/{image}.jpg";
//                                        try
//                                        {
//                                            var savedPath = Path.Combine(DataManager.TempPath, Guid.NewGuid() + ".jpg");
//                                            var imageDownloadRequest = new RestRequest(imageUrl, Method.Get);
//                                            using (var imgStream = await client.DownloadStreamAsync(imageDownloadRequest))
//                                            using(var fileStream = File.OpenWrite(savedPath))
//                                            {
//                                                await imgStream.CopyToAsync(fileStream);
//                                            }

//                                            messages.Add(new ImageMessage(path: savedPath, isTemp:false));
//                                            tempImagePaths.Add(savedPath);
//                                        }
//                                        catch(Exception e)
//                                        {
//                                            _logger.Error(e, "image downloading failed, image:{0}", imageUrl);
//                                        }
//                                    }
//                                }

//                                foreach (var sor in room.Value)
//                                {
//                                    MessageSource source;
//                                    var sorModel = sor.Value;
//                                    if (sorModel.Source == MessageSourceType.Group)
//                                        source = MessageSource.FromGroup(null, sorModel.SourceId, null);
//                                    else if (sorModel.Source == MessageSourceType.Guild)
//                                    {
//                                        var ids = sorModel.SourceId.Split('+');
//                                        if (ids.Length < 2)
//                                            continue;

//                                        source = MessageSource.FromGuild(null, ids[0], ids[1], null);
//                                    }
//                                    else if (sorModel.Source == MessageSourceType.Friend)
//                                    {
//                                        source = MessageSource.FromFriend(sorModel.SourceId, null);
//                                    }
//                                    else
//                                        continue;


//                                    MessageManager.SendToSource(source, messages);
//                                    await Task.Delay(10000);
//                                    if(tempImagePaths!=null && tempImagePaths.Any())
//                                    {
//                                        foreach(var file in tempImagePaths)
//                                        {
//                                            try
//                                            {
//                                                File.Delete(file);
//                                            }
//                                            catch
//                                            {
//                                                //ignore
//                                            }
//                                        }
//                                    }
//                                }
//                            }
//                            catch (Exception e)
//                            {
//                                _logger.Error(e, "weibo failed to send msg");
//                            }
//                            finally
//                            {
//                                await Task.Delay(intervalSpan);
//                            }
//                        }
//                    }
//                    await Task.WhenAny(Task.Delay(loopSpan), _completionSource.Task);
//                }
//                _logger.Info("loop finished");
//            }
//            catch(Exception e)
//            {
//                _logger.Error(e, "weibo loop error");
//            }
//        }
//    }
//}
