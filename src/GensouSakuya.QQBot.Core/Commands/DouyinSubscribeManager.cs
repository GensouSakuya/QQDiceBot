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
    [Command("douyin")]
    public class DouyinSubscribeManager : BaseManager
    {
        private static readonly  Logger _logger = Logger.GetLogger<DouyinSubscribeManager>();

        public override async System.Threading.Tasks.Task ExecuteAsync(MessageSource source, List<string> command, List<BaseMessage> originMessage, UserInfo qq, Group group, GroupMember member, GuildUserInfo guildUser, GuildMember guildmember)
        {
            SubscribeModel sbm;
            if (source.Type == MessageSourceType.Group)
            {
                if (member.QQ != DataManager.Instance.AdminQQ)
                {
                    MessageManager.SendToSource(source, "目前只有机器人管理员可以配置该功能哦");
                    return;
                }

                sbm = new SubscribeModel
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

                sbm = new SubscribeModel
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

                sbm = new SubscribeModel
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
                var sub = Subscribers.GetOrAdd(roomId, new ConcurrentDictionary<string,SubscribeModel>());
                if (sub.ContainsKey(sbm.ToString()))
                {
                    MessageManager.SendToSource(source, "该直播间已订阅");
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
        
        private static ConcurrentDictionary<string, ConcurrentDictionary<string, SubscribeModel>> _subscribers { get; set; }
        public static ConcurrentDictionary<string, ConcurrentDictionary<string, SubscribeModel>> Subscribers
        {
            get => _subscribers;
            set
            {
                if (value == null)
                {
                    _subscribers = new ConcurrentDictionary<string, ConcurrentDictionary<string, SubscribeModel>>();
                }
                else
                {
                    _subscribers = value;
                }
            }
        }

        static CancellationTokenSource _cancellationTokenSource;
        static DouyinSubscribeManager()
        {
            _notFireAgainList = new ConcurrentDictionary<string, string>();
            _cancellationTokenSource = new CancellationTokenSource();
            Task.Run(() => LoopCheck(_cancellationTokenSource.Token));
        }

        private static TaskCompletionSource<bool> _completionSource = new TaskCompletionSource<bool>();

        private static ConcurrentDictionary<string, string> _notFireAgainList;
        private static async Task LoopCheck(CancellationToken token)
        {
            try
            {
                await Task.Delay(5000);
                var loopSpan = new TimeSpan(0, 1, 0);
                var intervalSpan = new TimeSpan(0, 0, 10);
                var templateUrl = "https://live.douyin.com/webcast/room/web/enter/?aid=6383&app_name=douyin_web&live_id=1&device_platform=web&language=zh-CN&cookie_enabled=true&screen_width=2048&screen_height=1152&browser_language=zh-CN&browser_platform=Win32&browser_name=Edge&browser_version=119.0.0.0&web_rid={0}";
                while (!token.IsCancellationRequested)
                {
                    if (_completionSource.Task.IsCompleted)
                        _completionSource = new TaskCompletionSource<bool>();
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
                            _logger.Error(e, "douyin refresh cookies error");
                        }
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
                                    _logger.Info("douyin[{0}] start sending notice", room.Key);

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
                                    if(_notFireAgainList.TryRemove(room.Key, out _))
                                    {
                                        _logger.Info("douyin[{0}] flag removed", room.Key);
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                _logger.Error(e, "douyin failed to send msg");
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
                _logger.Error(e, "douyin loop error");
            }

        }

        internal enum StreamType
        {
            PC=0,
            Radio=1
        }
    }

    public class SubscribeModel
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
