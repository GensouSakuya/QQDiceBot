using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using GensouSakuya.QQBot.Core.Base;
using GensouSakuya.QQBot.Core.Model;
using GensouSakuya.QQBot.Core.PlatformModel;
using net.gensousakuya.dice;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace GensouSakuya.QQBot.Core.Commands
{
    [Command(".douyin")]
    public class DouyinSubscribeManager : BaseManager
    {
        private static readonly  Logger _logger = Logger.GetLogger<DouyinSubscribeManager>();

        public override async System.Threading.Tasks.Task ExecuteAsync(MessageSource source, List<string> command, List<BaseMessage> originMessage, UserInfo qq, Group group, GroupMember member, GuildUserInfo guildUser, GuildMember guildmember)
        {
            var toGroup = 0L;
            var fromQQ = 0L;

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
            if (!int.TryParse(command[1], out var roomId))
            {
                return;
            }

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
                if(sub.Remove(sub.ToString(), out _))
                {
                    MessageManager.SendToSource(source, "取消订阅成功！");
                    DataManager.Instance.NoticeConfigUpdated();
                }
                return;
            }

            return;
        }
        
        private static ConcurrentDictionary<int, ConcurrentDictionary<string, SubscribeModel>> _subscribers { get; set; }
        public static ConcurrentDictionary<int, ConcurrentDictionary<string, SubscribeModel>> Subscribers
        {
            get => _subscribers;
            set
            {
                if (value == null)
                {
                    _subscribers = new ConcurrentDictionary<int, ConcurrentDictionary<string, SubscribeModel>>();
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
            _notFireAgainList = new ConcurrentDictionary<int, int>();
            _cancellationTokenSource = new CancellationTokenSource();
            Task.Run(() => LoopCheck(_cancellationTokenSource.Token));
        }

        private static ConcurrentDictionary<int, int> _notFireAgainList;
        private static async Task LoopCheck(CancellationToken token)
        {
            var loopSpan = new TimeSpan(0, 1, 0);
            var intervalSpan = new TimeSpan(0, 0, 10);
            var templateUrl = "https://live.douyin.com/webcast/room/web/enter/?aid=6383&app_name=douyin_web&live_id=1&device_platform=web&language=zh-CN&cookie_enabled=true&screen_width=1920&screen_height=1080&browser_language=zh-CN&browser_platform=Win32&browser_name=Edge&browser_version=119.0.0.0&web_rid={0}&room_id_str=&enter_source=&is_need_double_stream=false&msToken=09yWBtC5V1hXl-MM3UZCf_THYyWPEMxFrYKWiqr88XQtmrehd4wGURmRQbmcsXFEk_MroU2PLrtMR8SiV40N4Wnt8KHY8JxUhO3qdwR4q2NgR55BCrwij77gEfk=&X-Bogus=DFSzswVOASTANGzZtFi3XcppgiuD&_signature=_02B4Z6wo00001QiInWQAAIDA.fd3-gS6a6kIiJnAACdxvJupNwAI-gKPJ6zh2uJHnBpcg1cbeAzYwXspv9DQFHZwLKfjdgAQb31YLFPLl0KiZ6wSJjv7labNG397PgdfO1CChO5Yco33CYDv3d";
            while (!token.IsCancellationRequested)
            {
                using (var client = new HttpClient())
                {
                    foreach(var room in Subscribers)
                    {
                        if (room.Value.Count <= 0)
                            continue;

                        try
                        {
                            var url = string.Format(templateUrl, room.Key);
                            var res = await client.GetAsync(url);
                            if (!res.IsSuccessStatusCode)
                                continue;
                            var content = await res.Content.ReadAsStringAsync();
                            var jsonRes = Newtonsoft.Json.JsonConvert.DeserializeObject(content);
                            var jobj = JObject.FromObject(jsonRes);
                            var status = jobj["data"]["data"][0]["status"].Value<int>();
                            var name = jobj["data"]["user"]["nickname"];
                            if (status == 2)
                            {
                                if (_notFireAgainList.ContainsKey(room.Key))
                                    continue;
                                _notFireAgainList.TryAdd(room.Key, room.Key);

                                foreach (var sor in room.Value)
                                {
                                    MessageSource source;
                                    var sorModel = sor.Value;
                                    if (sorModel.Source == MessageSourceType.Group)
                                        source = MessageSource.FromGroup(null, sorModel.SourceId, null);
                                    else if (sorModel.Source == MessageSourceType.Guild)
                                    {
                                        var ids = sorModel.SourceId.Split('+');
                                        if (ids.Length <= 2)
                                            continue;

                                        source = MessageSource.FromGuild(null, ids[0], ids[1], null);
                                    }
                                    else
                                        continue;

                                    MessageManager.SendToSource(source, $"{name}开播了！\nhttps://live.douyin.com/{room.Key}");
                                    await Task.Delay(2000);
                                }
                            }
                            else
                            {
                                _notFireAgainList.TryRemove(room.Key, out _);
                            }
                        }
                        catch(Exception e)
                        {
                            _logger.Error(e, "douyin loop error");
                        }
                        finally
                        {
                            await Task.Delay(intervalSpan);
                        }
                    }
                }

                await Task.Delay(loopSpan);
            }
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
