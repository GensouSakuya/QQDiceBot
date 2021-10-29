using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using GensouSakuya.QQBot.Core.Base;
using GensouSakuya.QQBot.Core.Model;
using GensouSakuya.QQBot.Core.PlatformModel;
using net.gensousakuya.dice;

namespace GensouSakuya.QQBot.Core.Commands
{
    [Command("todayhis")]
    public class TodayHistoryManager : BaseManager
    {
        private static readonly Logger _logger = Logger.GetLogger<TodayHistoryManager>();

        public override async System.Threading.Tasks.Task ExecuteAsync(List<string> command, List<BaseMessage> originMessage, MessageSourceType sourceType, UserInfo qq, Group group, GroupMember member)
        {
            var fromQQ = 0L;
            var toGroup = 0L;
            //var message = "";
            if (sourceType != MessageSourceType.Group)
            {
                return;
            }

            fromQQ = member.QQ;
            toGroup = member.GroupNumber;
            var permit = member.PermitType;
            if (!command.Any())
            {
                if (!GroupTodayHistoryConfig.TryGetValue(toGroup, out var config))
                {
                    MessageManager.SendTextMessage(MessageSourceType.Group, "当前群尚未开启历史上的今天功能", fromQQ, toGroup);
                    return;
                }
            }
            else
            {
                if (command[0].Equals("on", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (permit == PermitType.None)
                    {
                        MessageManager.SendTextMessage(MessageSourceType.Group, "只有群主或管理员才有权限开启历史上的今天功能", fromQQ, toGroup);
                        return;
                    }

                    UpdateGroupTodayHistoryConfig(toGroup, true);
                    MessageManager.SendTextMessage(MessageSourceType.Group, "历史上的今天功能已开启", fromQQ, toGroup);
                    return;
                }
                else if (command[0].Equals("off", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (permit == PermitType.None)
                    {
                        MessageManager.SendTextMessage(MessageSourceType.Group, "只有群主或管理员才有权限关闭历史上的今天功能", fromQQ, toGroup);
                        return;
                    }

                    UpdateGroupTodayHistoryConfig(toGroup, false);
                    MessageManager.SendTextMessage(MessageSourceType.Group, "历史上的今天已关闭", fromQQ, toGroup);
                    return;
                }
            }

            using (var client = new HttpClient())
            {
                var url = "https://news.topurl.cn/api";
                var res = await client.GetAsync(url);
                if (!res.IsSuccessStatusCode)
                {
                    MessageManager.SendTextMessage(sourceType, "请求失败了QAQ", fromQQ, toGroup);
                    return;
                }

                var strContent = await res.Content.ReadAsStringAsync();
                var jsonRes = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(strContent, new
                {
                    data = new
                    {
                        historyList = new List<TodayHistoryModel>()
                    }
                });

                if(!(jsonRes?.data?.historyList?.Any() ?? false))
                {
                    MessageManager.SendTextMessage(sourceType, "没找到数据捏", fromQQ, toGroup);
                    return;
                }

                var news = jsonRes.data.historyList.Take(4).ToList();
                var message = new List<string>();
                news.ForEach(n =>
                {
                    message.Add($"{n.Event}");
                });

                MessageManager.SendTextMessage(sourceType, string.Join("\n",message), fromQQ, toGroup);
            }
        }

        private static ConcurrentDictionary<long, bool> _groupTodayHistoryConfig = new ConcurrentDictionary<long, bool>();
        public static ConcurrentDictionary<long, bool> GroupTodayHistoryConfig
        {
            get => _groupTodayHistoryConfig;
            set
            {
                if (value == null)
                {
                    _groupTodayHistoryConfig = new ConcurrentDictionary<long, bool>();
                }
                else
                {
                    _groupTodayHistoryConfig = value;
                }
            }
        }

        public void UpdateGroupTodayHistoryConfig(long toGroup, bool enable)
        {
            if (enable)
                GroupTodayHistoryConfig.AddOrUpdate(toGroup, enable, (p, q) => enable);
            else
                GroupTodayHistoryConfig.TryRemove(toGroup, out _);
            DataManager.Instance.NoticeConfigUpdated();
        }

        public class TodayHistoryModel
        {
            public string Event { get; set; }
        }
    }
}
