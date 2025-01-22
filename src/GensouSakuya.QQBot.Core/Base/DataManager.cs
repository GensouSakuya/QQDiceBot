using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using GensouSakuya.QQBot.Core.Commands;
using GensouSakuya.QQBot.Core.Commands.V2;
using GensouSakuya.QQBot.Core.Handlers;
using GensouSakuya.QQBot.Core.Model;
using GensouSakuya.QQBot.Core.QQManager;
using Microsoft.Extensions.Logging;

namespace GensouSakuya.QQBot.Core.Base
{
    internal class DataManager
    {
        private readonly Subject<string> _observedLogList = new Subject<string>();
        private readonly ILogger _logger;
        public ConfigData Config { get; private set; }
        public static long QQ { get; private set; }

        public static string DataPath { get; private set; }
        private static string ConfigFilePath => Path.Combine(DataPath, Consts.ConfigFileName);
        public static string TempPath => Path.Combine(DataPath, ".temp");

        public DataManager(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<DataManager>();
            _observedLogList.Buffer(TimeSpan.FromMinutes(5), 2)
                .Where(x => x.Count > 0)
                .Select(list => Observable.FromAsync(() => this.Save()))
                .Concat()
                .Subscribe();
            NoticeConfigUpdatedAction = this.NoticeConfigUpdated;
        }

        public void NoticeConfigUpdated()
        {
            _observedLogList.OnNext("");
        }

        public async Task Init(long qq, string dataPath = null)
        {
            DataPath = string.IsNullOrWhiteSpace(dataPath) ? "data" : dataPath;
            _logger.LogInformation("数据目录：{0}", DataPath);
            QQ = qq;
            await Load();
            
            Instance = Config;

            if (EventCenter.GetGroupMemberList != null)
                await GroupMemberManager.StartLoadTask();
            if(EventCenter.GetQQInfo != null)
                await UserManager.StartLoadTask();
        }

        private void RefreshData()
        {
            Config.GroupMembers = GroupMemberManager.GroupMembers.Values.OrderBy(p => p.GroupNumber).ThenBy(p => p.QQ).ToList();
            Config.GroupBakiConfig = BakiManager.GroupBakiConfig;
            Config.Users = UserManager.Users.Values.OrderBy(p=>p.QQ).ToList();
            Config.GroupShaDiaoTuConfig = ShaDiaoTuManager.GroupShaDiaoTuConfig;
            Config.GroupRepeatConfig = RepeatManager.GroupRepeatConfig;
            Config.GroupTodayHistoryConfig = TodayHistoryManager.GroupTodayHistoryConfig;
            Config.GroupNewsConfig = NewsManager.GroupNewsConfig;
            Config.GroupHentaiCheckConfig = HentaiCheckManager.GroupHentaiCheckConfig;
            Config.GroupIgnore = IgnoreManager.GroupIgnore;
            Config.RuipingSentences = RuipingCommander.RuipingSentences;
            Config.DouyinSubscribers = DouyinSubscribeManager.Subscribers;
            Config.BiliLiveSubscribers = BilibiliLiveSubscribeManager.Subscribers;
        }

        private void UpdateData()
        {
            Config.BiliSpaceSubscribers ??= new ConcurrentDictionary<string, ConcurrentDictionary<string, SubscribeModel>>();
            Config.WeiboSubscribers ??= new ConcurrentDictionary<string, ConcurrentDictionary<string, SubscribeModel>>();
            Config.GroupBan ??= new ConcurrentDictionary<(long, long), string>();
            Config.QQBan ??= new ConcurrentDictionary<long, string>();
            Config.AiEnableConifig ??= new ConcurrentDictionary<long, bool>();
            Config.AiConfig ??= new AiConfig();

            GroupMemberManager.GroupMembers = new ConcurrentDictionary<(long, long), GroupMember>();
            Config.GroupMembers?.ForEach(p =>
            {
                GroupMemberManager.GroupMembers.AddOrUpdate((p.QQ, p.GroupNumber), p, (key, q) => p);
            });
            BakiManager.GroupBakiConfig = Config.GroupBakiConfig;
            UserManager.Users = new ConcurrentDictionary<long, UserInfo>();
            Config.Users?.ForEach(p =>
            {
                UserManager.Users.AddOrUpdate(p.QQ, p, (key, q) => p);
            });
            ShaDiaoTuManager.GroupShaDiaoTuConfig = Config.GroupShaDiaoTuConfig;
            RepeatManager.GroupRepeatConfig = Config.GroupRepeatConfig;
            TodayHistoryManager.GroupTodayHistoryConfig = Config.GroupTodayHistoryConfig;
            NewsManager.GroupNewsConfig = Config.GroupNewsConfig;
            HentaiCheckManager.GroupHentaiCheckConfig = Config.GroupHentaiCheckConfig;
            IgnoreManager.GroupIgnore = Config.GroupIgnore;
            RuipingCommander.RuipingSentences = Config.RuipingSentences;
            DouyinSubscribeManager.Subscribers = Config.DouyinSubscribers;
            BilibiliLiveSubscribeManager.Subscribers = Config.BiliLiveSubscribers;
        }

        public Task Save()
        {
            _logger.LogInformation("saving data");
            this.RefreshData();
            var path = ConfigFilePath;
            try
            {
                var dir = Path.GetDirectoryName(path);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                var data = Tools.SerializeObject(Config);

                File.WriteAllText(path, data);
                _logger.LogDebug("Config updated");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "save config error");
            }
            return Task.CompletedTask;
        }

        public async Task Load()
        {
            var path = ConfigFilePath;
            _logger.LogDebug("loading from:" + path);
            if (File.Exists(path))
            {
                var text = await File.ReadAllTextAsync(path);
                try
                {
                    var data = Tools.DeserializeObject<ConfigData>(text);
                    Config = data;
                    this.UpdateData();
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "ConfigLoadError");
                }
            }
            else
            {
                _logger.LogDebug("not found {0}, generate new config file", path);
                Config = new ConfigData();
                this.UpdateData();
                await this.Save();
            }

            if (!Directory.Exists(TempPath))
            {
                Directory.CreateDirectory(TempPath);
            }
        }

        public async Task Stop()
        {
            await Save();
        }

        [Obsolete("改为使用ServiceProvider生成的")]
        internal static ConfigData Instance { get; set; }

        [Obsolete("改为使用ServiceProvider生成的DataManager")]
        internal static Action NoticeConfigUpdatedAction;
    }
}
