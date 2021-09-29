using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using GensouSakuya.QQBot.Core.Commands;
using GensouSakuya.QQBot.Core.Model;
using GensouSakuya.QQBot.Core.QQManager;

namespace GensouSakuya.QQBot.Core.Base
{
    public class DataManager
    {
        private readonly Subject<string> _observedLogList = new Subject<string>();
        private static readonly Logger _logger = Logger.GetLogger<DataManager>();

        private DataManager()
        {
            _observedLogList.Buffer(TimeSpan.FromMinutes(5), 2)
                .Where(x => x.Count > 0)
                .Select(list => Observable.FromAsync(() => DataManager.Save()))
                .Concat()
                .Subscribe();
        }

        private string _botName = "骰娘";

        public string BotName
        {
            get => _botName;
            set
            {
                if (value == null)
                {
                    _botName = "骰娘";
                }
                else
                {
                    _botName = value;
                }
            }
        }

        public long AdminQQ { get; set; }

        public List<long> DisabledJrrpGroupNumbers { get; set; } = new List<long>();
        public List<long> EnabledRandomImgNumbers { get; set; } = new List<long>();

        public void NoticeConfigUpdated()
        {
            _observedLogList.OnNext("");
        }

        public ConcurrentDictionary<long, RepeatConfig> GroupRepeatConfig
        {
            get => RepeatManager.GroupRepeatConfig;
            set => RepeatManager.GroupRepeatConfig = value;
        }

        public ConcurrentDictionary<long, ShaDiaoTuConfig> GroupShaDiaoTuConfig
        {
            get => ShaDiaoTuManager.GroupShaDiaoTuConfig;
            set => ShaDiaoTuManager.GroupShaDiaoTuConfig = value;
        }

        public ConcurrentDictionary<long, string> QQBan
        {
            get => BanManager.QQBan;
            set => BanManager.QQBan = value;
        }

        public ConcurrentDictionary<(long, long), string> GroupBan
        {
            get => BanManager.GroupBan;
            set => BanManager.GroupBan = value;
        }

        public List<GroupMember> GroupMembers
        {
            private get => GroupMemberManager.GroupMembers.Values.ToList();
            set
            {
                GroupMemberManager.GroupMembers = new ConcurrentDictionary<(long, long), GroupMember>();
                value?.ForEach(p =>
                {
                    GroupMemberManager.GroupMembers.TryAdd((p.QQ, p.GroupNumber), p);
                });
            }
        }

        public List<UserInfo> Users
        {
            get => UserManager.Users;
            set => UserManager.Users = value;
        }

        public ConcurrentDictionary<long, BakiConfig> GroupBakiConfig
        {
            get => BakiManager.GroupBakiConfig;
            set => BakiManager.GroupBakiConfig = value;
        }

        public static async Task Init()
        {
            var path = Config.ConfigFile;
            _logger.Debug("InitPath:" + path);
            if (File.Exists(path))
            {
                var xml = File.ReadAllText(path);
                try
                {
                    var db = Tools.DeserializeObject<DataManager>(xml);
                    Instance = db;
                }
                catch (Exception e)
                {
                    _logger.Error(e, "ConfigLoadError");
                }
            }
            else
            {
                _logger.Debug("not found" + path);
            }

            if (Instance == null)
            {
                Instance = new DataManager();
            }

            await GroupMemberManager.StartLoadTask(System.Threading.CancellationToken.None);
        }

        public static Task Save()
        {
            var path = Config.ConfigFile;
            try
            {
                var dir = Path.GetDirectoryName(path);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                File.WriteAllText(path, Tools.SerializeObject(Instance));
                _logger.Debug("Config updated");
            }
            catch (Exception e)
            {
                _logger.Error(e, "save config error");
            }
            return Task.CompletedTask;
        }

        public static async Task Stop()
        {
            await Save();
        }

        public static DataManager Instance { get; set; }

    }
}
