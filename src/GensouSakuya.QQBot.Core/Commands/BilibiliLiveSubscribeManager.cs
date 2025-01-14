using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GensouSakuya.QQBot.Core.Base;
using GensouSakuya.QQBot.Core.Helpers;
using GensouSakuya.QQBot.Core.Model;
using GensouSakuya.QQBot.Core.PlatformModel;
using net.gensousakuya.dice;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using RestSharp;

namespace GensouSakuya.QQBot.Core.Commands
{
    [Command("bililive")]
    public class BilibiliLiveSubscribeManager : BaseManager
    {
        private static readonly Logger _logger = Logger.GetLogger<BilibiliLiveSubscribeManager>();

        public override async Task ExecuteAsync(MessageSource source, List<string> command, List<BaseMessage> originMessage, UserInfo qq, Group group, GroupMember member, GuildUserInfo guildUser, GuildMember guildmember)
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
            else if (source.Type == MessageSourceType.Guild)
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
            else if (source.Type == MessageSourceType.Friend)
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

            if (command.Count < 1)
            {
                return;
            }

            var first = command[0];
            if (first == "trigger")
            {
                _completionSource.TrySetResult(true);
            }
            else
            {
                if (command.Count < 2)
                {
                    return;
                }
                var roomId = command[1];
                if (first == "subscribe")
                {
                    var sub = Subscribers.GetOrAdd(roomId, new ConcurrentDictionary<string, SubscribeModel>());
                    if (sub.ContainsKey(sbm.ToString()))
                    {
                        MessageManager.SendToSource(source, "该直播间已订阅");
                        return;
                    }

                    sub[sbm.ToString()] = sbm;
                    MessageManager.SendToSource(source, "订阅成功！");
                    DataManager.NoticeConfigUpdatedAction();
                    return;
                }
                else if (first == "unsubscribe")
                {
                    if (!Subscribers.TryGetValue(roomId, out var sub))
                    {
                        return;
                    }
                    if (sub.Remove(sbm.ToString(), out _))
                    {
                        MessageManager.SendToSource(source, "取消订阅成功！");
                        DataManager.NoticeConfigUpdatedAction();
                    }
                    return;
                }
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
        static BilibiliLiveSubscribeManager()
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
                await Task.WhenAny(Task.Delay(5000), _completionSource.Task);
                var loopSpan = new TimeSpan(0, 1, 0);
                var intervalSpan = new TimeSpan(0, 0, 10);
                var apiTemplateUrl = "https://api.live.bilibili.com/room/v1/Room/room_init?id={0}";
                while (!token.IsCancellationRequested)
                {
                    if (_completionSource.Task.IsCompleted)
                        _completionSource = new TaskCompletionSource<bool>();

                    using (var client = new RestClient())
                    {
                        client.AddDefaultHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36 Edg/119.0.0.0");
                        foreach (var room in Subscribers)
                        {
                            if (room.Value.Count <= 0)
                                continue;

                            try
                            {
                                var url = string.Format(apiTemplateUrl, room.Key);
                                var res = await client.ExecuteAsync(new RestRequest(url, Method.Get));
                                if (!res.IsSuccessStatusCode)
                                {
                                    _logger.Error(res.ErrorException, "get roominfo failed");
                                    continue;
                                }
                                var content = res.Content;

                                var jsonRes = Newtonsoft.Json.JsonConvert.DeserializeObject(content);
                                var jobj = JObject.FromObject(jsonRes);
                                var isStreaming = jobj["data"]["live_status"].Value<int>() == 1;
                                if (isStreaming)
                                {
                                    if (_notFireAgainList.ContainsKey(room.Key))
                                        continue;
                                    _notFireAgainList.TryAdd(room.Key, room.Key);

                                    var uid = jobj["data"]["uid"].Value<ulong>();
                                    var info = BiliLiveHelper.GetBiliLiveInfo(uid);

                                    _logger.Info("bililive[{0}] start sending notice", room.Key);

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
                                        _logger.Info("bililive[{0}] flag removed", room.Key);
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                _logger.Error(e, "bililive failed to send msg");
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
            catch (Exception e)
            {
                _logger.Error(e, "bililive loop error");
            }

        }
    }
}
