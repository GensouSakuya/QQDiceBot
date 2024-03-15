using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GensouSakuya.QQBot.Core.Base;
using GensouSakuya.QQBot.Core.Model;
using GensouSakuya.QQBot.Core.PlatformModel;
using net.gensousakuya.dice;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace GensouSakuya.QQBot.Core.Commands
{
    [Command("weibo")]
    public class WeiboSubscribeManager : BaseManager
    {
        private static readonly  Logger _logger = Logger.GetLogger<WeiboSubscribeManager>();

        public override async System.Threading.Tasks.Task ExecuteAsync(MessageSource source, List<string> command, List<BaseMessage> originMessage, UserInfo qq, Group group, GroupMember member, GuildUserInfo guildUser, GuildMember guildmember)
        {
            WeiboSubscribeModel sbm;
            if (source.Type == MessageSourceType.Group)
            {
                if (member.QQ != DataManager.Instance.AdminQQ)
                {
                    MessageManager.SendToSource(source, "目前只有机器人管理员可以配置该功能哦");
                    return;
                }

                sbm = new WeiboSubscribeModel
                {
                    Source = MessageSourceType.Group,
                    SourceId = source.GroupId
                };
            }
            else if(source.Type == MessageSourceType.Guild)
            {
                if (guildmember.UserId != DataManager.Instance.AdminGuildUserId)
                {
                    MessageManager.SendToSource(source, "目前只有机器人管理员可以配置该功能哦");
                    return;
                }

                sbm = new WeiboSubscribeModel
                {
                    Source = MessageSourceType.Guild,
                    SourceId = $"{source.GuildId}+{source.ChannelId}"
                };
            }
            else if(source.Type == MessageSourceType.Friend)
            {
                if (source.QQ != DataManager.Instance.AdminQQ.ToString())
                {
                    MessageManager.SendToSource(source, "目前只有机器人管理员可以配置该功能哦");
                    return;
                }

                sbm = new WeiboSubscribeModel
                {
                    Source = MessageSourceType.Friend,
                    SourceId = source.QQ
                };
            }
            else
            {
                MessageManager.SendToSource(source, "懒得支持！");
                return;
            }

            if(command.Count < 2)
            {
                return;
            }

            var first = command[0];
            var roomId = command[1];

            if (first == "subscribe")
            {
                var sub = Subscribers.GetOrAdd(roomId, new ConcurrentDictionary<string, WeiboSubscribeModel>());
                if (sub.ContainsKey(sbm.ToString()))
                {
                    MessageManager.SendToSource(source, "该微博已订阅");
                    return;
                }

                sub[sbm.ToString()] = sbm;
                MessageManager.SendToSource(source, "订阅成功！");
                DataManager.Instance.NoticeConfigUpdated();
                return;
            }
            else if (first == "unsubscribe")
            {
                if(!Subscribers.TryGetValue(roomId, out var sub))
                {
                    return;
                }
                if(sub.Remove(sbm.ToString(), out _))
                {
                    MessageManager.SendToSource(source, "取消订阅成功！");
                    DataManager.Instance.NoticeConfigUpdated();
                }
                return;
            }
            else if(first == "trigger")
            {
                _completionSource.TrySetResult(true);
            }

            return;
        }
        
        private static ConcurrentDictionary<string, ConcurrentDictionary<string, WeiboSubscribeModel>> _subscribers { get; set; }
        public static ConcurrentDictionary<string, ConcurrentDictionary<string, WeiboSubscribeModel>> Subscribers
        {
            get => _subscribers;
            set
            {
                if (value == null)
                {
                    _subscribers = new ConcurrentDictionary<string, ConcurrentDictionary<string, WeiboSubscribeModel>>();
                }
                else
                {
                    _subscribers = value;
                }
            }
        }

        static CancellationTokenSource _cancellationTokenSource;
        static WeiboSubscribeManager()
        {
            _lastWeiboId = new ConcurrentDictionary<string, string>();
            _cancellationTokenSource = new CancellationTokenSource();
            Task.Run(() => LoopCheck(_cancellationTokenSource.Token));
        }

        private static TaskCompletionSource<bool> _completionSource = new TaskCompletionSource<bool>();

        private static ConcurrentDictionary<string, string> _lastWeiboId;
        private static async Task LoopCheck(CancellationToken token)
        {
            try
            {
                await Task.Delay(5000);
                var loopSpan = new TimeSpan(0, 10, 0);
                var intervalSpan = new TimeSpan(0, 0, 10);
                var templateUrl = "https://m.weibo.cn/api/container/getIndex?type=uid&value={0}";
                while (!token.IsCancellationRequested)
                {
                    if (_completionSource.Task.IsCompleted)
                        _completionSource = new TaskCompletionSource<bool>();
                    using (var client = new RestClient())
                    {
                        //try
                        //{
                        //    client.AddDefaultHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36 Edg/119.0.0.0");
                        //    //set cookie
                        //    await client.GetAsync(new RestRequest("https://live.douyin.com"));
                        //}
                        //catch (Exception e)
                        //{
                        //    _logger.Error(e, "douyin refresh cookies error");
                        //}
                        foreach (var room in Subscribers)
                        {
                            if (room.Value.Count <= 0)
                                continue;

                            try
                            {
                                var url = string.Format(templateUrl, room.Key);
                                var res = await client.GetAsync(new RestRequest(url));
                                if (!res.IsSuccessStatusCode)
                                {
                                    _logger.Error(res.ErrorException, "get roominfo failed");
                                    continue;
                                }
                                var content = res.Content;
                                var jobj = JObject.FromObject(content);
                                var name = jobj["data"]["userInfo"]["screen_name"];
                                var containerid = jobj["data"]["tabsInfo"]["tabs"][1]["containerid"];
                                res = await client.ExecuteGetAsync(new RestRequest(url + "&containerid=" + containerid));
                                content = res.Content;
                                jobj = JObject.Parse(content);
                                var weibos = jobj["data"]["cards"];
                                var newest = weibos[0];
                                var id = newest["mblog"]["id"].ToString();
                                var text = newest["mblog"]["text"];
                                if (!_lastWeiboId.ContainsKey(room.Key))
                                {
                                    _lastWeiboId[room.Key] = id;
                                    continue;
                                }
                                else if (_lastWeiboId[room.Key] == id)
                                {
                                    continue;
                                }

                                _lastWeiboId[room.Key] = id;
                                _logger.Info("weibo[{0}] start sending notice", room.Key);

                                string msg = $"【{name}】发布了微博：{text}";

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
                            catch (Exception e)
                            {
                                _logger.Error(e, "weibo failed to send msg");
                            }
                            finally
                            {
                                await Task.Delay(intervalSpan);
                            }
                        }
                    }
                    await Task.WhenAny(Task.Delay(loopSpan), _completionSource.Task);
                }
                _logger.Error("loop finished");
            }
            catch(Exception e)
            {
                _logger.Error(e, "weibo loop error");
            }

        }
    }

    public class WeiboSubscribeModel
    {
        public MessageSourceType Source { get; set; }
        public string SourceId { get; set; }

        public override bool Equals(object obj)
        {
            return obj is SubscribeModel model &&
                   Source == model.Source &&
                   SourceId == model.SourceId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Source, SourceId);
        }

        public override string ToString()
        {
            return $"{Source}:{SourceId}";
        }
    }
}
