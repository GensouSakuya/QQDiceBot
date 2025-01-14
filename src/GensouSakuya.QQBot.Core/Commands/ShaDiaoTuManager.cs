﻿using System;
using System.Collections.Concurrent;
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
        private static readonly Dictionary<long, DateTime> _lastTime = new Dictionary<long, DateTime>();
        private static Random _rand = new Random();
        private static string ShaDiaoImagePath => Path.Combine(DataManager.DataPath, "沙雕图");
        public override async Task ExecuteAsync(MessageSource source, List<string> command, List<BaseMessage> originMessage, UserInfo qq, Group group, GroupMember member, GuildUserInfo guildUser, GuildMember guildmember)
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
            if (!command.Any())
            {
                if (!GroupShaDiaoTuConfig.TryGetValue(toGroup, out var config))
                {
                    MessageManager.SendTextMessage(MessageSourceType.Group, "当前群尚未开启沙雕图功能", fromQQ, toGroup);
                }
                else
                {
                    MessageManager.SendTextMessage(MessageSourceType.Group, $"当前沙雕图概率：{config.Percent}%", fromQQ, toGroup);
                }

                return;
            }

            if (command[0].Equals("on", StringComparison.CurrentCultureIgnoreCase))
            {
                if (!member.IsGroupAdmin() && !Tools.IsRobotAdmin(fromQQ))
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


                UpdateGroupShaDiaoTuConfig(toGroup, config);
                MessageManager.SendTextMessage(MessageSourceType.Group, $"随机沙雕图已开启，发图概率：{config.Percent}%", fromQQ, toGroup);
            }
            else if (command[0].Equals("off", StringComparison.CurrentCultureIgnoreCase))
            {
                if (!member.IsGroupAdmin() && !Tools.IsRobotAdmin(fromQQ))
                {
                    MessageManager.SendTextMessage(MessageSourceType.Group, "只有群主或管理员才有权限关闭沙雕图功能", fromQQ, toGroup);
                    return;
                }

                UpdateGroupShaDiaoTuConfig(toGroup, null);
                MessageManager.SendTextMessage(MessageSourceType.Group, "随机沙雕图已关闭", fromQQ, toGroup);
            }
            else if (command[0].Equals("add", StringComparison.CurrentCultureIgnoreCase))
            {
                if (!GroupShaDiaoTuConfig.ContainsKey(toGroup))
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

                    var savedPath = Path.Combine(ShaDiaoImagePath, Guid.NewGuid().ToString() + ".png");
                    if (!Directory.Exists(ShaDiaoImagePath))
                    {
                        Directory.CreateDirectory(ShaDiaoImagePath);
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
                var dir = ShaDiaoImagePath;
                if (!Directory.Exists(dir))
                    return;
                var files = Directory.GetFiles(dir);
                if (!files.Any())
                    return;
                var fileName = files[_rand.Next(0, files.Length)];
                MessageManager.SendImageMessage(source.Type, fileName, fromQQ, toGroup);
            }
        }

        private static ConcurrentDictionary<long, ShaDiaoTuConfig> _groupShaDiaoTuConfig = new ConcurrentDictionary<long, ShaDiaoTuConfig>();
        public static ConcurrentDictionary<long, ShaDiaoTuConfig> GroupShaDiaoTuConfig
        {
            get => _groupShaDiaoTuConfig;
            set
            {
                if (value == null)
                {
                    _groupShaDiaoTuConfig = new ConcurrentDictionary<long, ShaDiaoTuConfig>();
                }
                else
                {
                    _groupShaDiaoTuConfig = value;
                }
            }
        }

        public void UpdateGroupShaDiaoTuConfig(long toGroup, ShaDiaoTuConfig config)
        {
            if(config != null)
                GroupShaDiaoTuConfig.AddOrUpdate(toGroup, config, (p, q) => config);
            else
                GroupShaDiaoTuConfig.TryRemove(toGroup, out _);
            DataManager.NoticeConfigUpdatedAction();
        }
    }

    public class ShaDiaoTuConfig
    {
        public int Percent { get; set; }
    }
}
