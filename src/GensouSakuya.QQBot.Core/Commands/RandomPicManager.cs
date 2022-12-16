using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using GensouSakuya.QQBot.Core.Base;
using GensouSakuya.QQBot.Core.Model;
using GensouSakuya.QQBot.Core.PlatformModel;
using net.gensousakuya.dice;

namespace GensouSakuya.QQBot.Core.Commands
{
    [Command("setu")]
    public class RandomPicManager : BaseManager
    {
        private static readonly  Logger _logger = Logger.GetLogger<RandomPicManager>();
        private static Dictionary<Tuple<MessageSourceType,long>, DateTime> _lastFetchTimeDic = new Dictionary<Tuple<MessageSourceType,long>, DateTime>();
        private const int _intervalSeconds = 60;

        public override async System.Threading.Tasks.Task ExecuteAsync(MessageSource source, List<string> command, List<BaseMessage> originMessage, UserInfo qq, Group group, GroupMember member, GuildUserInfo guildUser, GuildMember guildmember)
        {
            var toGroup = 0L;
            var fromQQ = 0L;
            if (source.Type == MessageSourceType.Private)
            {
                fromQQ = qq.QQ;
                if (fromQQ != DataManager.Instance.AdminQQ)
                {
                    MessageManager.SendToSource(source, "不给看不给看！");
                    return;
                }
            }
            else if (source.Type == MessageSourceType.Group)
            {
                fromQQ = member.QQ;
                toGroup = member.GroupNumber;
                if (command.Any() && command.First() == "on")
                {
                    if (member.QQ == DataManager.Instance.AdminQQ)
                    {
                        if (!DataManager.Instance.EnabledRandomImgNumbers.Contains(member.GroupNumber))
                        {
                            DataManager.Instance.EnabledRandomImgNumbers.Add(member.GroupNumber);
                            MessageManager.SendToSource(source, "启用成功！");
                            DataManager.Instance.NoticeConfigUpdated();
                            return;
                        }
                    }
                }
                else if (command.Any() && command.First() == "off")
                {
                    if (member.QQ == DataManager.Instance.AdminQQ)
                    {
                        if (DataManager.Instance.EnabledRandomImgNumbers.Contains(member.GroupNumber))
                        {
                            DataManager.Instance.EnabledRandomImgNumbers.Remove(member.GroupNumber);
                            MessageManager.SendToSource(source, "禁用成功！");
                            DataManager.Instance.NoticeConfigUpdated();
                            return;
                        }
                    }
                }

                if (!DataManager.Instance.EnabledRandomImgNumbers.Contains(member.GroupNumber))
                {
                    MessageManager.SendTextMessage(source.Type, "这个群没启用这个功能，快去找开发者来开启", fromQQ, toGroup);
                    return;
                }
            }
            else
            {
                return;
            }

            if (command.Count > 2)
            {
                MessageManager.SendToSource(source, "Tag太多啦，一次最多只能查两个");
                return;
            }

            var key = new Tuple<MessageSourceType, long>(source.Type, source.Type == MessageSourceType.Private ? fromQQ : toGroup);
            if (fromQQ != DataManager.Instance.AdminQQ)
            {
                if (_lastFetchTimeDic.ContainsKey(key))
                {
                    if (DateTime.Now.Subtract(_lastFetchTimeDic[key]).TotalSeconds < _intervalSeconds)
                    {
                        MessageManager.SendToSource(source, "太频繁啦，每分钟只能出一张图");
                        return;
                    }
                }
            }

            using (var client = new HttpClient())
            {
                var url = "https://danbooru.donmai.us/posts/random.json";
                var tag = string.Join("+", command);
                if ((command?.Count ?? 0) > 0)
                {
                    url += $"?tags={tag}";
                }
                var res = await client.GetAsync(url);
                if (res.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    MessageManager.SendToSource(source, $"{tag}:\ntag写错了吗，没找到图呢");
                    return;
                }
                if (!res.IsSuccessStatusCode)
                {
                    MessageManager.SendToSource(source, $"{tag}:\n请求失败了QAQ");
                    return;
                }

                var strContent = await res.Content.ReadAsStringAsync();
                var jsonRes = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(strContent,new
                {
                    file_url = "",
                    id = 0L,
                    success = (bool?)null,
                    is_banned = (bool?)null
                });
                if (jsonRes.success.HasValue && !jsonRes.success.Value)
                {
                    MessageManager.SendToSource(source, $"{tag}:\ntag写错了吗，没找到图呢");
                    return;
                }

                if (jsonRes.is_banned.HasValue && jsonRes.is_banned.Value)
                {
                    MessageManager.SendToSource(source, $"{tag}:\nid:{jsonRes.id}\n这张图被作者要求下架了QAQ");
                    return;
                }

                try
                {
                    var imgRes = await client.GetAsync(jsonRes.file_url);
                    var img = System.Drawing.Image.FromStream(await imgRes.Content.ReadAsStreamAsync());
                    var fileName = jsonRes.file_url.Split('/').Last();

                    var dir = Path.Combine(Config.DataPath, "image");
                    var path = Path.Combine(dir, fileName);
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }

                    img.Save(path);
                    MessageManager.SendToSource(source, $"[CQ:image,file={fileName}]\n{tag}:\nhttps://danbooru.donmai.us/posts/{jsonRes.id}");
                    File.Delete(path);
                }
                catch (Exception e)
                {
                    _logger.Error(e, $"download remote image failed, detail response: {Environment.NewLine}{Newtonsoft.Json.JsonConvert.SerializeObject(jsonRes)}");
                    throw;
                }

                if (fromQQ != DataManager.Instance.AdminQQ)
                {
                    if (_lastFetchTimeDic.ContainsKey(key))
                    {
                        _lastFetchTimeDic[key] = DateTime.Now;
                    }
                    else
                    {
                        _lastFetchTimeDic.Add(key, DateTime.Now);
                    }
                }
            }
            return;
        }
    }
}
