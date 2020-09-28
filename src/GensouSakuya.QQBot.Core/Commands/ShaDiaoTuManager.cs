using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GensouSakuya.QQBot.Core.Base;
using GensouSakuya.QQBot.Core.Model;
using GensouSakuya.QQBot.Core.PlatformModel;
using net.gensousakuya.dice;
using Group = GensouSakuya.QQBot.Core.Model.Group;

namespace GensouSakuya.QQBot.Core.Commands
{
    [Command("shadiao")]
    public class ShaDiaoTuManager : BaseManager
    {
        private static readonly Regex _imageGuid = new Regex(@"\[QQ:pic=(?<Guid>.*?)\]");
        private static readonly Dictionary<long, DateTime> _lastTime = new Dictionary<long, DateTime>();
        private static Random _rand = new Random();
        public override async Task ExecuteAsync(List<string> command, List<BaseMessage> originMessage, MessageSourceType sourceType, UserInfo qq, Group group, GroupMember member)
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
                return;
            }

            if (command[0].Equals("on", StringComparison.CurrentCultureIgnoreCase))
            {
                if (permit == PermitType.None)
                {
                    MessageManager.SendTextMessage(MessageSourceType.Group, "只有群主或管理员才有权限开启沙雕图功能", fromQQ, toGroup);
                    return;
                }
                ShaDiaoTuConfig config;

                if (command.Count == 1)
                {
                    config = new ShaDiaoTuConfig();
                }
                else
                {
                    if (int.TryParse(command[1], out var percent))
                    {
                        config = new ShaDiaoTuConfig
                        {
                            Percent = percent > 100 ? 100 : percent
                        };
                    }
                    else
                    {
                        config = new ShaDiaoTuConfig();
                    }
                }


                DataManager.Instance.GroupShaDiaoTuConfig.AddOrUpdate(toGroup, config, (p, q) => config);
                MessageManager.SendTextMessage(MessageSourceType.Group, $"随机沙雕图已开启，发图概率：{config.Percent}%", fromQQ, toGroup);
            }
            else if (command[0].Equals("off", StringComparison.CurrentCultureIgnoreCase))
            {
                if (permit == PermitType.None)
                {
                    MessageManager.SendTextMessage(MessageSourceType.Group, "只有群主或管理员才有权限关闭沙雕图功能", fromQQ, toGroup);
                    return;
                }

                DataManager.Instance.GroupShaDiaoTuConfig.TryRemove(toGroup, out _);
                MessageManager.SendTextMessage(MessageSourceType.Group, "随机沙雕图已关闭", fromQQ, toGroup);
            }
            else if (command[0].Equals("add", StringComparison.CurrentCultureIgnoreCase))
            {
                if (!DataManager.Instance.GroupShaDiaoTuConfig.ContainsKey(toGroup))
                {
                    MessageManager.SendTextMessage(MessageSourceType.Group, "先找人把功能打开啦", fromQQ, toGroup);
                    return;
                }

                if (_lastTime.ContainsKey(fromQQ) && _lastTime[fromQQ] == DateTime.Today &&
                    fromQQ != DataManager.Instance.AdminQQ)
                {
                    MessageManager.SendTextMessage(MessageSourceType.Group, "每人每天只能添加一张", fromQQ, toGroup);
                    return;
                }

                var image = (ImageMessage) originMessage.Find(p => p is ImageMessage);

                if (image == null)
                {
                    MessageManager.SendTextMessage(MessageSourceType.Group, "图呢0 0", fromQQ, toGroup);
                    return;
                }
                //var img = command[1];
                //var match = _imageGuid.Match(img);
                //if (!match.Groups["Guid"].Success)
                //{
                //    MessageManager.SendTextMessage(MessageSourceType.Group, "图呢0 0", fromQQ, toGroup);
                //    return;
                //}

                //var fileName = match.Groups["Guid"].Value;
                //var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp", "image");
                //var iniFileName = Path.Combine(path, Path.GetFileNameWithoutExtension(fileName) + ".ini");
                //if (!File.Exists(iniFileName))
                //{
                //    MessageManager.SendTextMessage(MessageSourceType.Group, "上传失败惹", fromQQ, toGroup);
                //    return;
                //}

                //var fileText = File.ReadAllText(iniFileName);
                //var url = fileText.Split('\n').FirstOrDefault(p => p.StartsWith("url"))?.Substring(4);
                var url = image.Url;
                if (string.IsNullOrWhiteSpace(url))
                {
                    MessageManager.SendTextMessage(MessageSourceType.Group, "上传失败惹", fromQQ, toGroup);
                    return;
                }

                using (var client = new HttpClient())
                {
                    var imgRes = await client.GetAsync(url);
                    var imgItem = System.Drawing.Image.FromStream(await imgRes.Content.ReadAsStreamAsync());

                    var savedPath = Path.Combine(Config.ShaDiaoImagePath, Guid.NewGuid().ToString() + ".png");
                    if (!Directory.Exists(Config.ShaDiaoImagePath))
                    {
                        Directory.CreateDirectory(Config.ShaDiaoImagePath);
                    }

                    imgItem.Save(savedPath);
                }

                MessageManager.SendTextMessage(MessageSourceType.Group, "上传成功", fromQQ, toGroup);
                if (!_lastTime.ContainsKey(fromQQ))
                {
                    _lastTime.Add(fromQQ, DateTime.Today);
                }
                else
                {
                    _lastTime[fromQQ] = DateTime.Today;
                }
            }
            else if (command[0].Equals("shadiaotu", StringComparison.CurrentCultureIgnoreCase))
            {
                var dir = Path.Combine(Config.DataPath, "沙雕图");
                if (!Directory.Exists(dir))
                    return;
                var files = Directory.GetFiles(dir);
                if (!files.Any())
                    return;
                var fileName = files[_rand.Next(0, files.Length)];
                MessageManager.SendImageMessage(sourceType, fileName, fromQQ, toGroup);
            }
        }
    }

    public class ShaDiaoTuConfig
    {
        public int Percent { get; set; }
    }
}
