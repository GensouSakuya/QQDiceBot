using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using GensouSakuya.QQBot.Core.Commands;
using GensouSakuya.QQBot.Core.Commands.V2;
using GensouSakuya.QQBot.Core.Model;
using GensouSakuya.QQBot.Core.QQManager;

namespace GensouSakuya.QQBot.Core.Base
{
    public class DataManager
    {
        private readonly Subject<string> _observedLogList = new Subject<string>();
        private static readonly Logger _logger = Logger.GetLogger<DataManager>();
        public static long QQ { get; private set; }

        public static string DataPath { get; private set; }
        private static string ConfigFilePath => Path.Combine(DataPath, Consts.ConfigFileName);
        public static string TempPath => Path.Combine(DataPath, ".temp");

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
            set => _botName = value ?? "骰娘";
        }

        public long AdminQQ { get; set; }
        public string AdminGuildUserId { get; set; }

        public List<long> DisabledJrrpGroupNumbers { get; set; } = new List<long>();
        public List<long> EnabledRandomImgNumbers { get; set; } = new List<long>();

        public void NoticeConfigUpdated()
        {
            _observedLogList.OnNext("");
        }

        public ConcurrentDictionary<long, RepeatConfig> GroupRepeatConfig { get; set; }

        public ConcurrentDictionary<long, ShaDiaoTuConfig> GroupShaDiaoTuConfig { get; set; }

        public ConcurrentDictionary<string, ConcurrentDictionary<string, SubscribeModel>> DouyinSubscribers { get; set; }

        public ConcurrentDictionary<string, ConcurrentDictionary<string, SubscribeModel>> WeiboSubscribers { get; set; }
        public ConcurrentDictionary<string, ConcurrentDictionary<string, SubscribeModel>> BiliLiveSubscribers { get; set; }
        public ConcurrentDictionary<string, ConcurrentDictionary<string, SubscribeModel>> BiliSpaceSubscribers { get; set; }

        public ConcurrentDictionary<long, string> QQBan { get; set; }

        public ConcurrentDictionary<(long, long), string> GroupBan { get; set; }
        public ConcurrentDictionary<(long, long), string> GroupIgnore { get; set; }

        public List<GroupMember> GroupMembers { get; set; }

        public List<UserInfo> Users { get; set; }

        public ConcurrentDictionary<long, BakiConfig> GroupBakiConfig { get; set; }

        public ConcurrentDictionary<long, bool> GroupTodayHistoryConfig { get; set; }
        public ConcurrentDictionary<long, bool> GroupNewsConfig { get; set; }
        public ConcurrentDictionary<long, bool> GroupHentaiCheckConfig { get; set; }

        public List<string> RuipingSentences { get; set; }

        public QWenConfig QWenConfig { get; set; }
        public ConcurrentDictionary<long, bool> GroupQWenConfig { get; set; }
        public QWenLimit QWenLimig { get; set; }

        public static async Task Init(long qq, string dataPath = null)
        {
            DataPath = string.IsNullOrWhiteSpace(dataPath) ? "data" : dataPath;
            _logger.Info("数据目录：{0}", DataPath);
            QQ = qq;
            await Load();

            if (EventCenter.GetGroupMemberList != null)
                await GroupMemberManager.StartLoadTask();
            if(EventCenter.GetQQInfo != null)
                await UserManager.StartLoadTask();
        }

        private void RefreshData()
        {
            GroupMembers = GroupMemberManager.GroupMembers.Values.OrderBy(p => p.GroupNumber).ThenBy(p => p.QQ).ToList();
            GroupBakiConfig = BakiManager.GroupBakiConfig;
            Users = UserManager.Users.Values.OrderBy(p=>p.QQ).ToList();
            GroupBan = BanManager.GroupBan;
            QQBan = BanManager.QQBan;
            GroupShaDiaoTuConfig = ShaDiaoTuManager.GroupShaDiaoTuConfig;
            GroupRepeatConfig = RepeatManager.GroupRepeatConfig;
            GroupTodayHistoryConfig = TodayHistoryManager.GroupTodayHistoryConfig;
            GroupNewsConfig = NewsManager.GroupNewsConfig;
            GroupHentaiCheckConfig = HentaiCheckManager.GroupHentaiCheckConfig;
            GroupIgnore = IgnoreManager.GroupIgnore;
            RuipingSentences = RuipingCommander.RuipingSentences;
            DouyinSubscribers = DouyinSubscribeManager.Subscribers;
            WeiboSubscribers = WeiboSubscribeManager.Subscribers;
            BiliLiveSubscribers = BilibiliLiveSubscribeManager.Subscribers;
        }

        private void UpdateData()
        {
            GroupQWenConfig ??= new ConcurrentDictionary<long, bool>();
            QWenConfig ??= new QWenConfig();
            QWenLimig ??= new QWenLimit();
            BiliSpaceSubscribers ??= new ConcurrentDictionary<string, ConcurrentDictionary<string, SubscribeModel>>();

            GroupMemberManager.GroupMembers = new ConcurrentDictionary<(long, long), GroupMember>();
            GroupMembers?.ForEach(p =>
            {
                GroupMemberManager.GroupMembers.AddOrUpdate((p.QQ, p.GroupNumber), p, (key, q) => p);
            });
            BakiManager.GroupBakiConfig = GroupBakiConfig;
            UserManager.Users = new ConcurrentDictionary<long, UserInfo>();
            Users?.ForEach(p =>
            {
                UserManager.Users.AddOrUpdate(p.QQ, p, (key, q) => p);
            });
            BanManager.GroupBan = GroupBan;
            BanManager.QQBan = QQBan;
            ShaDiaoTuManager.GroupShaDiaoTuConfig = GroupShaDiaoTuConfig;
            RepeatManager.GroupRepeatConfig = GroupRepeatConfig;
            TodayHistoryManager.GroupTodayHistoryConfig = GroupTodayHistoryConfig;
            NewsManager.GroupNewsConfig = GroupNewsConfig;
            HentaiCheckManager.GroupHentaiCheckConfig = GroupHentaiCheckConfig;
            IgnoreManager.GroupIgnore = GroupIgnore;
            RuipingCommander.RuipingSentences = RuipingSentences;
            DouyinSubscribeManager.Subscribers = DouyinSubscribers;
            WeiboSubscribeManager.Subscribers = WeiboSubscribers;
            BilibiliLiveSubscribeManager.Subscribers = BiliLiveSubscribers;
        }

        public static Task Save()
        {
            _logger.Info("saving data");
            Instance.RefreshData();
            var path = ConfigFilePath;
            try
            {
                var dir = Path.GetDirectoryName(path);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                var data = Tools.SerializeObject(Instance);

                File.WriteAllText(path, data);
                _logger.Debug("Config updated");
            }
            catch (Exception e)
            {
                _logger.Error(e, "save config error");
            }
            return Task.CompletedTask;
        }

        public static async Task Load()
        {
            var path = ConfigFilePath;
            _logger.Debug("loading from:" + path);
            if (File.Exists(path))
            {
                var text = await File.ReadAllTextAsync(path);
                try
                {
                    var db = Tools.DeserializeObject<DataManager>(text);
                    Instance = db;
                    Instance.UpdateData();
                }
                catch (Exception e)
                {
                    _logger.Error(e, "ConfigLoadError");
                }
            }
            else
            {
                _logger.Debug("not found {0}, generate new config file", path);
                Instance = new DataManager();
                Instance.UpdateData();
                await DataManager.Save();
            }

            if (!Directory.Exists(TempPath))
            {
                Directory.CreateDirectory(TempPath);
            }
        }

        public static async Task Stop()
        {
            await Save();
        }

        public static DataManager Instance { get; set; }

    }
}
