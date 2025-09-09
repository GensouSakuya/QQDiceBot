using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using GensouSakuya.QQBot.Core.Base;
using GensouSakuya.QQBot.Core.Interfaces;
using GensouSakuya.QQBot.Core.Model;
using GensouSakuya.QQBot.Core.PlatformModel;
using Microsoft.Extensions.Logging;
using net.gensousakuya.dice;

namespace GensouSakuya.QQBot.Core.Commands
{
    [Command("todayhis")]
    public class TodayHistoryHandler : IMessageCommandHandler
    {
        private readonly ILogger _logger;
        private readonly DataManager _dataManager;
        public TodayHistoryHandler(ILogger<TodayHistoryHandler> logger, DataManager dataManager)
        {
            _logger = logger;
            _dataManager = dataManager;
        }

        public void UpdateGroupTodayHistoryConfig(long toGroup, bool enable)
        {
            if (enable)
                _dataManager.Config.GroupTodayHistoryConfig.AddOrUpdate(toGroup, enable, (p, q) => enable);
            else
                _dataManager.Config.GroupTodayHistoryConfig.TryRemove(toGroup, out _);
            _dataManager.NoticeConfigUpdated();
        }

        public async Task<bool> ExecuteAsync(MessageSource source, IEnumerable<string> command, List<BaseMessage> originMessage, SourceFullInfo sourceInfo)
        {
            var fromQQ = 0L;
            var toGroup = 0L;
            //var message = "";
            if (source.Type != MessageSourceType.Group)
            {
                return false;
            }
            var member = sourceInfo.GroupMember;
            fromQQ = member.QQ;
            toGroup = member.GroupNumber;
            if (!command.Any())
            {
                if (!_dataManager.Config.GroupTodayHistoryConfig.TryGetValue(toGroup, out var config))
                {
                    MessageManager.SendTextMessage(MessageSourceType.Group, "当前群尚未开启历史上的今天功能", fromQQ, toGroup);
                    return true;
                }
            }
            else
            {
                if (command.ElementAt(0).Equals("on", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (!member.IsGroupAdmin() && !Tools.IsRobotAdmin(fromQQ))
                    {
                        MessageManager.SendTextMessage(MessageSourceType.Group, "只有群主或管理员才有权限开启历史上的今天功能", fromQQ, toGroup);
                        return true;
                    }

                    UpdateGroupTodayHistoryConfig(toGroup, true);
                    MessageManager.SendTextMessage(MessageSourceType.Group, "历史上的今天功能已开启", fromQQ, toGroup);
                    return true;
                }
                else if (command.ElementAt(0).Equals("off", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (!member.IsGroupAdmin() && !Tools.IsRobotAdmin(fromQQ))
                    {
                        MessageManager.SendTextMessage(MessageSourceType.Group, "只有群主或管理员才有权限关闭历史上的今天功能", fromQQ, toGroup);
                        return true;
                    }

                    UpdateGroupTodayHistoryConfig(toGroup, false);
                    MessageManager.SendTextMessage(MessageSourceType.Group, "历史上的今天已关闭", fromQQ, toGroup);
                    return true;
                }
            }

            using (var client = new HttpClient())
            {
                var url = "https://news.topurl.cn/api";
                var res = await client.GetAsync(url);
                if (!res.IsSuccessStatusCode)
                {
                    MessageManager.SendTextMessage(source.Type, "请求失败了QAQ", fromQQ, toGroup);
                    return true;
                }

                var strContent = await res.Content.ReadAsStringAsync();
                var jsonRes = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(strContent, new
                {
                    data = new
                    {
                        historyList = new List<TodayHistoryModel>()
                    }
                });

                if (!(jsonRes?.data?.historyList?.Any() ?? false))
                {
                    MessageManager.SendTextMessage(source.Type, "没找到数据捏", fromQQ, toGroup);
                    return true;
                }

                var news = jsonRes.data.historyList.Take(4).ToList();
                var message = new List<string>();
                news.ForEach(n =>
                {
                    message.Add($"{n.Event}");
                });

                MessageManager.SendTextMessage(source.Type, string.Join("\n", message), fromQQ, toGroup);
            }
            return true;
        }

        public class TodayHistoryModel
        {
            public string Event { get; set; }
        }
    }
}
