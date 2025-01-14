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
    [Command("news")]
    public class NewsManager : BaseManager
    {
        private static readonly Logger _logger = Logger.GetLogger<NewsManager>();

        public override async System.Threading.Tasks.Task ExecuteAsync(MessageSource source, List<string> command, List<BaseMessage> originMessage, UserInfo qq, Group group, GroupMember member, GuildUserInfo guildUser, GuildMember guildmember)
        {
            var fromQQ = 0L;
            var toGroup = 0L;
            //var message = "";
            if (source.Type != MessageSourceType.Group)
            {
                return;
            }

            fromQQ = member.QQ;
            toGroup = member.GroupNumber;
            var permit = member.PermitType;
            if (!command.Any())
            {
                if (!GroupNewsConfig.TryGetValue(toGroup, out var config))
                {
                    MessageManager.SendTextMessage(MessageSourceType.Group, "当前群尚未开启新闻功能", fromQQ, toGroup);
                    return;
                }
            }
            else
            {
                if (command[0].Equals("on", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (!member.IsGroupAdmin() && !Tools.IsRobotAdmin(fromQQ))
                    {
                        MessageManager.SendTextMessage(MessageSourceType.Group, "只有群主或管理员才有权限开启新闻功能", fromQQ, toGroup);
                        return;
                    }

                    UpdateGroupNewsConfig(toGroup, true);
                    MessageManager.SendTextMessage(MessageSourceType.Group, "新闻功能已开启", fromQQ, toGroup);
                    return;
                }
                else if (command[0].Equals("off", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (!member.IsGroupAdmin() && !Tools.IsRobotAdmin(fromQQ))
                    {
                        MessageManager.SendTextMessage(MessageSourceType.Group, "只有群主或管理员才有权限关闭新闻功能", fromQQ, toGroup);
                        return;
                    }

                    UpdateGroupNewsConfig(toGroup, false);
                    MessageManager.SendTextMessage(MessageSourceType.Group, "新闻功能已关闭", fromQQ, toGroup);
                    return;
                }
            }

            using (var client = new HttpClient())
            {
                var url = "https://news.topurl.cn/api";
                var res = await client.GetAsync(url);
                if (!res.IsSuccessStatusCode)
                {
                    MessageManager.SendTextMessage(source.Type, "请求失败了QAQ", fromQQ, toGroup);
                    return;
                }

                var strContent = await res.Content.ReadAsStringAsync();
                var jsonRes = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(strContent, new
                {
                    data = new
                    {
                        newsList = new List<NewsModel>()
                    }
                });

                if(!(jsonRes?.data?.newsList?.Any() ?? false))
                {
                    MessageManager.SendTextMessage(source.Type, "没找到新闻捏", fromQQ, toGroup);
                    return;
                }

                var news = jsonRes.data.newsList.Take(4).ToList();
                var message = new List<string>();
                news.ForEach(n =>
                {
                    message.Add($"{n.Title}:{n.Url}");
                });

                MessageManager.SendTextMessage(source.Type, string.Join("\n",message), fromQQ, toGroup);
            }
        }

        private static ConcurrentDictionary<long, bool> _groupNewsConfig = new ConcurrentDictionary<long, bool>();
        public static ConcurrentDictionary<long, bool> GroupNewsConfig
        {
            get => _groupNewsConfig;
            set
            {
                if (value == null)
                {
                    _groupNewsConfig = new ConcurrentDictionary<long, bool>();
                }
                else
                {
                    _groupNewsConfig = value;
                }
            }
        }

        public void UpdateGroupNewsConfig(long toGroup, bool enable)
        {
            if (enable)
                GroupNewsConfig.AddOrUpdate(toGroup, enable, (p, q) => enable);
            else
                GroupNewsConfig.TryRemove(toGroup, out _);
            DataManager.NoticeConfigUpdatedAction();
        }

        public class NewsModel
        {
            public string Title { get; set; }
            public string Url { get; set; }
        }
    }
}
