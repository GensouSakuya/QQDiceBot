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
            }
            else if (sourceType == EventSourceType.Group)
            {
                if (command.Any() && command.First() == "on")
                {
                    if (member.PermitType != Native.Csharp.Sdk.Cqp.Enum.PermitType.None)
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
                    if (member.PermitType != Native.Csharp.Sdk.Cqp.Enum.PermitType.None)
                    {
                        if (DataManager.Instance.EnabledRandomImgNumbers.Contains(member.GroupNumber))
                        {
                            DataManager.Instance.EnabledRandomImgNumbers.Remove(member.GroupNumber);
                            MessageManager.Send(sourceType, "禁用成功！", fromQQ, toGroup);
                            return;
                        }
                    }
                }
                else if (!DataManager.Instance.EnabledRandomImgNumbers.Contains(member.GroupNumber))
                {
                    MessageManager.Send(sourceType, "本群没启用这个功能，快去找管理员开启", fromQQ, toGroup);
                    return;
                }
                fromQQ = member.QQ;
                toGroup = member.GroupNumber;
            }
            else
            {
                return;
            }

            var key = new Tuple<EventSourceType, long>(sourceType, sourceType == EventSourceType.Private ? fromQQ : toGroup);
            if (_lastFetchTimeDic.ContainsKey(key))
            {
                if (DateTime.Now.Subtract(_lastFetchTimeDic[key]).TotalSeconds < _intervalSeconds)
                {
                    MessageManager.Send(sourceType, "太频繁啦，每分钟只能出一张图", fromQQ, toGroup);
                    return;
                }
            }

            using (var client = new HttpClient())
            {
                var url = "https://danbooru.donmai.us/posts/random.json";
                if ((command?.Count ?? 0) > 0)
                {
                    url += $"?tags={string.Join("+", command)}";
                }
                var res = await client.GetAsync("https://danbooru.donmai.us/posts/random.json");
                if (!res.IsSuccessStatusCode)
                {
                    MessageManager.Send(sourceType, "请求失败了QAQ", fromQQ, toGroup);
                    return;
                }

                var strContent = await res.Content.ReadAsStringAsync();
                var jsonRes = Newtonsoft.Json .JsonConvert.DeserializeAnonymousType(strContent,new
                {
                    file_url = "",
                    id = 0L,
                    success = (bool?)null,
                });
                if (jsonRes.success.HasValue && !jsonRes.success.Value)
                {
                    MessageManager.Send(sourceType, "tag写错了吗，没找到图呢", fromQQ, toGroup);
                    return;
                }

                var imgRes = await client.GetAsync(jsonRes.file_url);
                var img = System.Drawing.Image.FromStream(await imgRes.Content.ReadAsStreamAsync());
                var fileName = jsonRes.file_url.Split('/').Last();
                var dir = Path.Combine(Common.AppDirectory, "image");
                var path = Path.Combine(dir, fileName);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                img.Save(path);
                MessageManager.Send(sourceType, $"[CQ:image,file={fileName}]https://danbooru.donmai.us/posts/{jsonRes.id}");
            }
            return;
        }
    }
}
