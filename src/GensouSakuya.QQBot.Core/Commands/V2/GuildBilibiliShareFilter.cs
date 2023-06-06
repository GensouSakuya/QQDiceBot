using GensouSakuya.QQBot.Core.Base;
using GensouSakuya.QQBot.Core.Model;
using GensouSakuya.QQBot.Core.PlatformModel;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace GensouSakuya.QQBot.Core.Commands.V2
{
    internal class GuildBilibiliShareFilter : BaseCommanderV2
    {
        public override async Task<bool> Check(MessageSource source, List<BaseMessage> originMessage, UserInfo qq, Group group, GroupMember member, GuildUserInfo guildUser, GuildMember guildmember)
        {
            await Task.Yield();

            if (source.IsTraditionSource)
                return false;
            if (guildUser == null || guildmember == null)
                return false;
            if (!(originMessage.FirstOrDefault() is JsonMessage jm))
                return false;
            if (string.IsNullOrWhiteSpace(jm.Json))
                return false;
            if (!jm.Json.Contains("哔哩哔哩") && !jm.Json.Contains("1109937557"))
                return false;
            return true;
        }

        public override async Task NextAsync(MessageSource source, List<BaseMessage> originMessage, UserInfo qq, Group group, GroupMember member, GuildUserInfo guildUser, GuildMember guildmember)
        {
            await Task.Yield();

            var jm = originMessage.FirstOrDefault() as JsonMessage;

            try
            {
                var json = WebUtility.HtmlDecode(jm.Json);
                var jobj = JObject.Parse(json);
                var title = jobj["meta"]["detail_1"]["desc"];
                var url = jobj["meta"]["detail_1"]["qqdocurl"];

                var sourceMessageId = (originMessage?.FirstOrDefault() as SourceMessage)?.Id ?? default;
                var messages = new List<BaseMessage>
                {
                    new TextMessage($"{guildmember.NickName}分享了B站视频\n{title}\n{url}")
                };
                MessageManager.SendToSource(source, messages);
                StopChain();
            }
            catch(Exception e)
            {
                //ignore
            }
        }
    }
}
