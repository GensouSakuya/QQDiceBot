using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GensouSakuya.QQBot.Core.Base;
using GensouSakuya.QQBot.Core.Model;
using GensouSakuya.QQBot.Core.PlatformModel;
using net.gensousakuya.dice;
using RestSharp;

namespace GensouSakuya.QQBot.Core.Commands
{
    [Command("hc")]
    public class HentaiCheckManager : BaseManager
    {
        private static readonly Logger _logger = Logger.GetLogger<HentaiCheckManager>();

        public override async System.Threading.Tasks.Task ExecuteAsync(MessageSource source, List<string> command, List<BaseMessage> originMessage, UserInfo qq, Group group, GroupMember member, GuildUserInfo guildUser, GuildMember guildmember)
        {
            var fromQQ = 0L;
            var toGroup = 0L;
            //var message = "";
            if (source.Type != MessageSourceType.Group)
            {
                return;
            }

            var sourceMessageId = (originMessage?.FirstOrDefault() as SourceMessage)?.Id ?? default;
            var messages = new List<BaseMessage>();
            messages.Add(new QuoteMessage(toGroup, fromQQ, sourceMessageId));

            fromQQ = member.QQ;
            toGroup = member.GroupNumber;
            var permit = member.PermitType;

            if (originMessage.Count <= 2)
            {
                if (command[0].Equals("on", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (permit == PermitType.None)
                    {
                        MessageManager.SendTextMessage(MessageSourceType.Group, "只有群主或管理员才有权限开启色图鉴定功能", fromQQ, toGroup);
                        return;
                    }

                    UpdateGroupHentaiCheckConfig(toGroup, true);
                    MessageManager.SendTextMessage(MessageSourceType.Group, "色图鉴定已开启", fromQQ, toGroup);
                    return;
                }
                else if (command[0].Equals("off", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (!member.IsGroupAdmin() && !Tools.IsRobotAdmin(fromQQ))
                    {
                        MessageManager.SendTextMessage(MessageSourceType.Group, "只有群主或管理员才有权限关闭色图鉴定功能", fromQQ, toGroup);
                        return;
                    }

                    UpdateGroupHentaiCheckConfig(toGroup, false);
                    MessageManager.SendTextMessage(MessageSourceType.Group, "色图鉴定已关闭", fromQQ, toGroup);
                    return;
                }
                else if (!GroupHentaiCheckConfig.TryGetValue(toGroup, out var config))
                {
                    MessageManager.SendTextMessage(MessageSourceType.Group, "当前群尚未色图鉴定功能", fromQQ, toGroup);
                    return;
                }
                else
                {
                    messages.Add(new TextMessage("图呢？"));
                    MessageManager.SendToSource(source, messages);
                    return;
                }
            }
            else
            {
                if (!(originMessage?.ElementAt(2) is ImageMessage im))
                {
                    messages.Add(new TextMessage("图呢？"));
                    MessageManager.SendToSource(source, messages);
                    return;
                }

                var imageUrl = im.Url;
                if (string.IsNullOrWhiteSpace(imageUrl))
                {
                    MessageManager.SendToSource(source, "图片上传失败惹");
                    return;
                }

                var client = new RestClient(new RestClientOptions
                {
                    MaxTimeout = -1
                });;
                var imageDownloadRequest = new RestRequest(imageUrl, Method.Get);
                var imgRes = await client.ExecuteAsync(imageDownloadRequest);
                if (!imgRes.IsSuccessful)
                {
                    MessageManager.SendToSource(source, "请求失败了QAQ");
                    return;
                }

                var savedPath = Path.Combine(Config.TempPath, Guid.NewGuid() + ".png");

                var uploadRequest = new RestRequest("https://checkimage.querydata.org/api", Method.Post)
                {
                    AlwaysMultipartFormData = true
                };
                uploadRequest.AddFile("image", imgRes.RawBytes, savedPath, contentType: imgRes.ContentType);
                var res = await client.ExecuteAsync(uploadRequest);

                if (!res.IsSuccessful)
                {
                    MessageManager.SendToSource(source, "请求失败了QAQ");
                    return;
                }

                var strContent = res.Content;
                var jsonRes =
                    Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(strContent, new List<HentaiCheckModel>());

                if (jsonRes == null || !jsonRes.Any())
                {
                    MessageManager.SendToSource(source, "请求失败了QAQ");
                    return;
                }

                var messageText = "没鉴定出来><";
                var result = jsonRes.OrderByDescending(p => p.Probability).FirstOrDefault();
                switch (result?.ClassName)
                {
                    case "Drawing":
                    case "Neutral":
                        messageText = "这不是挺健全的嘛";
                        break;
                    case "Hentai":
                        messageText = "色疯辣！";
                        break;
                    case "Sexy":
                        messageText = "我超，太烧啦！";
                        break;
                    case "Porn":
                        messageText = "你完了，我这就叫狗管理来抓你";
                        break;
                }

                messages.Add(new TextMessage(messageText));
                MessageManager.SendToSource(source, messages);
            }
        }

        private static ConcurrentDictionary<long, bool> _groupHentaiCheckConfig = new ConcurrentDictionary<long, bool>();
        public static ConcurrentDictionary<long, bool> GroupHentaiCheckConfig
        {
            get => _groupHentaiCheckConfig;
            set
            {
                if (value == null)
                {
                    _groupHentaiCheckConfig = new ConcurrentDictionary<long, bool>();
                }
                else
                {
                    _groupHentaiCheckConfig = value;
                }
            }
        }

        public void UpdateGroupHentaiCheckConfig(long toGroup, bool enable)
        {
            if (enable)
                _groupHentaiCheckConfig.AddOrUpdate(toGroup, enable, (p, q) => enable);
            else
                _groupHentaiCheckConfig.TryRemove(toGroup, out _);
            DataManager.Instance.NoticeConfigUpdated();
        }

        public class HentaiCheckModel
        {
            public string ClassName { get; set; }
            public decimal Probability { get; set; }
        }
    }
}
