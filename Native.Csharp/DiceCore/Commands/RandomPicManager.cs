using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using Native.Csharp.App;

namespace net.gensousakuya.dice
{
    public class RandomPicManager : BaseManager
    {
        private static Dictionary<Tuple<EventSourceType,long>, DateTime> _lastFetchTimeDic = new Dictionary<Tuple<EventSourceType,long>, DateTime>();
        private const int _intervalSeconds = 60;

        public override async System.Threading.Tasks.Task ExecuteAsync(List<string> command, EventSourceType sourceType, UserInfo qq, Group group, GroupMember member)
        {
            var toGroup = 0L;
            var fromQQ = 0L;
            if (sourceType == EventSourceType.Private)
            {
                fromQQ = qq.QQ;
                if (fromQQ != DataManager.Instance.AdminQQ)
                {
                    MessageManager.Send(sourceType, "不给看不给看！", fromQQ, toGroup);
                    return;
                }
            }
            else if (sourceType == EventSourceType.Group)
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
                            MessageManager.Send(sourceType, "启用成功！", fromQQ, toGroup);
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
                            MessageManager.Send(sourceType, "禁用成功！", fromQQ, toGroup);
                            return;
                        }
                    }
                }

                if (!DataManager.Instance.EnabledRandomImgNumbers.Contains(member.GroupNumber))
                {
                    MessageManager.Send(sourceType, "这个群没启用这个功能，快去找开发者来开启", fromQQ, toGroup);
                    return;
                }
            }
            else
            {
                return;
            }

            if (command.Count > 2)
            {
                MessageManager.Send(sourceType, "Tag太多啦，一次最多只能查两个", fromQQ, toGroup);
                return;
            }

            var key = new Tuple<EventSourceType, long>(sourceType, sourceType == EventSourceType.Private ? fromQQ : toGroup);
            if (fromQQ != DataManager.Instance.AdminQQ)
            {
                if (_lastFetchTimeDic.ContainsKey(key))
                {
                    if (DateTime.Now.Subtract(_lastFetchTimeDic[key]).TotalSeconds < _intervalSeconds)
                    {
                        MessageManager.Send(sourceType, "太频繁啦，每分钟只能出一张图", fromQQ, toGroup);
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
                    MessageManager.Send(sourceType, $"{tag}:\ntag写错了吗，没找到图呢", fromQQ, toGroup);
                    return;
                }
                if (!res.IsSuccessStatusCode)
                {
                    MessageManager.Send(sourceType, $"{tag}:\n请求失败了QAQ", fromQQ, toGroup);
                    return;
                }

                var strContent = await res.Content.ReadAsStringAsync();
                var jsonRes = Newtonsoft.Json .JsonConvert.DeserializeAnonymousType(strContent,new
                {
                    file_url = "",
                    id = 0L,
                    success = (bool?)null,
                    is_banned = (bool?)null
                });
                if (jsonRes.success.HasValue && !jsonRes.success.Value)
                {
                    MessageManager.Send(sourceType, $"{tag}:\ntag写错了吗，没找到图呢", fromQQ, toGroup);
                    return;
                }

                if (jsonRes.is_banned.HasValue && jsonRes.is_banned.Value)
                {
                    MessageManager.Send(sourceType, $"{tag}:\nid:{jsonRes.id}\n这张图被作者要求下架了:(", fromQQ, toGroup);
                    return;
                }

                try
                {
                    var imgRes = await client.GetAsync(jsonRes.file_url);
                    var img = System.Drawing.Image.FromStream(await imgRes.Content.ReadAsStreamAsync());
                    var fileName = jsonRes.file_url.Split('/').Last();

                    var dir = Path.Combine(Common.DataDirectory, "image");
                    var path = Path.Combine(dir, fileName);
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }

                    img.Save(path);
                    MessageManager.Send(sourceType, $"[CQ:image,file={fileName}]\n{tag}:\nhttps://danbooru.donmai.us/posts/{jsonRes.id}",
                        fromQQ, toGroup);
                    File.Delete(path);
                }
                catch (Exception e)
                {
                    FileLogHelper.WriteLog(e, Common.AppDirectory);
                    FileLogHelper.WriteLog(Newtonsoft.Json.JsonConvert.SerializeObject(jsonRes), Common.AppDirectory);
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
