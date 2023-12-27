using GensouSakuya.QQBot.Core.Base;
using GensouSakuya.QQBot.Core.Model;
using GensouSakuya.QQBot.Core.PlatformModel;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace GensouSakuya.QQBot.Core.Commands.V2
{
    internal class GuildShareFilter : BaseCommanderV2
    {
        private readonly static List<string> _supportApps = new List<string>
        {
            _miniappAppName,
            _structmsgAppName,
            _channelshareAppName
        };
        private const string _miniappAppName = "com.tencent.miniapp_01";
        private const string _structmsgAppName = "com.tencent.structmsg";
        private const string _channelshareAppName = "com.tencent.channel.share";

        public override async Task<bool> Check(MessageSource source, List<BaseMessage> originMessage, UserInfo qq, Group group, GroupMember member, GuildUserInfo guildUser, GuildMember guildmember)
        {
            await Task.Yield();

            try
            {
                if (source.IsTraditionSource)
                    return false;
                if (guildUser == null || guildmember == null)
                    return false;
                var jm = originMessage.FirstOrDefault(p => p is JsonMessage) as JsonMessage;
                if (jm == null)
                    return false;
                if (string.IsNullOrWhiteSpace(jm.Json))
                    return false;
                var json = WebUtility.HtmlDecode(jm.Json);
                var jobj = JObject.Parse(json);
                if (!_supportApps.Contains(jobj["app"]?.ToString()))
                    return false;
            }
            catch
            {
                return false;
            }
            return true;
        }

        public override async Task NextAsync(MessageSource source, List<BaseMessage> originMessage, UserInfo qq, Group group, GroupMember member, GuildUserInfo guildUser, GuildMember guildmember)
        {
            await Task.Yield();

            var jm = originMessage.FirstOrDefault(p => p is JsonMessage) as JsonMessage;

            try
            {
                var json = WebUtility.HtmlDecode(jm.Json);
                var jobj = JObject.Parse(json);

                var app = jobj["app"]?.ToString();
                if (_miniappAppName.Equals(app, StringComparison.OrdinalIgnoreCase))
                {
                    var name = jobj["meta"]["detail_1"]["title"];
                    var title = jobj["meta"]["detail_1"]["desc"];
                    var url = HandleUrl(jobj["meta"]["detail_1"]["qqdocurl"]?.ToString());

                    var messages = new List<BaseMessage>
                    {
                        new TextMessage($"{guildmember.NickName}分享了{name}的内容\n{title}\n{url}")
                    };
                    SendMessage(source, messages);
                }
                else if (_structmsgAppName.Equals(app, StringComparison.OrdinalIgnoreCase))
                {
                    var type = jobj["view"]?.ToString();
                    if (type == "news")
                    {
                        var tag = jobj["meta"]["news"]["tag"];
                        var title = jobj["meta"]["news"]["title"];
                        var url = HandleUrl(jobj["meta"]["news"]["jumpUrl"]?.ToString());

                        var messages = new List<BaseMessage>
                        {
                            new TextMessage($"{guildmember.NickName}分享了{tag}链接\n{title}\n{url}")
                        };
                        SendMessage(source, messages);
                    }
                }
                else if (_channelshareAppName.Equals(app, StringComparison.OrdinalIgnoreCase))
                {
                    var url = HandleUrl(jobj["meta"]["detail"]["link"]?.ToString());
                    var messages = new List<BaseMessage>
                    {
                        new TextMessage($"{guildmember.NickName}分享了链接\n{url}")
                    };
                    SendMessage(source, messages);
                }
            }
            catch (Exception e)
            {
                //ignore
            }
        }

        private void SendMessage(MessageSource source, List<BaseMessage> messages)
        {
            MessageManager.SendToSource(source, messages);
            StopChain();
        }

        static Random _rand = new Random();
        private string HandleUrl(string url)
        {
            if (url == null)
                return null;
            var trimedUrl = url;
            var trimStartCount = 0;
            if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            {
                trimStartCount = 7;
            }
            else if (url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                trimStartCount = 8;
            }
            trimedUrl = trimedUrl.Substring(trimStartCount, url.Length - trimStartCount);
            //var index1 = _rand.Next(trimedUrl.Length);
            //var index2 = _rand.Next(trimedUrl.Length);
            //var index3 = _rand.Next(trimedUrl.Length);
            //trimedUrl = trimedUrl.Insert(index1, "请");
            //trimedUrl = trimedUrl.Insert(index2, "删");
            //trimedUrl = trimedUrl.Insert(index3, "除");
            return trimedUrl;
        }
    }
}