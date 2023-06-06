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
                    var url = jobj["meta"]["detail_1"]["qqdocurl"];

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
                        var url = jobj["meta"]["news"]["jumpUrl"];

                        var messages = new List<BaseMessage>
                        {
                            new TextMessage($"{guildmember.NickName}分享了{tag}链接\n{title}\n{url}")
                        };
                        SendMessage(source, messages);
                    }
                }
                else if (_channelshareAppName.Equals(app, StringComparison.OrdinalIgnoreCase))
                {
                    var url = jobj["meta"]["detail"]["link"];
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
    }
}