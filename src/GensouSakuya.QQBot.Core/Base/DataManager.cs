using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GensouSakuya.QQBot.Core.Commands;
using GensouSakuya.QQBot.Core.Model;
using GensouSakuya.QQBot.Core.PlatformModel;
using GensouSakuya.QQBot.Core.QQManager;
using net.gensousakuya.dice;

namespace GensouSakuya.QQBot.Core.Base
{
    public class DataManager
    {
        private static volatile object locker = new object();

        private DataManager()
        { }

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

        private ConcurrentDictionary<long, RepeatConfig> _groupRepeatConfig = new ConcurrentDictionary<long, RepeatConfig>();
        public ConcurrentDictionary<long, RepeatConfig> GroupRepeatConfig
        {
            get => _groupRepeatConfig;
            set
            {
                if (value == null)
                {
                    _groupRepeatConfig = new ConcurrentDictionary<long, RepeatConfig>();
                }
                else
                {
                    _groupRepeatConfig = value;
                }
            }
        }

        private ConcurrentDictionary<long, ShaDiaoTuConfig> _groupShaDiaoTuConfig = new ConcurrentDictionary<long, ShaDiaoTuConfig>();
        public ConcurrentDictionary<long, ShaDiaoTuConfig> GroupShaDiaoTuConfig
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

        private ConcurrentDictionary<long,string> _qqBan = new ConcurrentDictionary<long, string>();
        public ConcurrentDictionary<long, string> QQBan
        {
            get => _qqBan;
            set
            {
                if (value == null)
                {
                    _qqBan = new ConcurrentDictionary<long, string>();
                }
                else
                {
                    _qqBan = value;
                }
            }
        }

        private ConcurrentDictionary<(long,long), string> _groupBan = new ConcurrentDictionary<(long, long), string>();
        public ConcurrentDictionary<(long, long), string> GroupBan
        {
            get => _groupBan;
            set
            {
                if (value == null)
                {
                    _groupBan = new ConcurrentDictionary<(long, long), string>();
                }
                else
                {
                    _groupBan = value;
                }
            }
        }

        public List<GroupMember> GroupMembers
        {
            get => GroupMemberManager.GroupMembers;
            set => GroupMemberManager.GroupMembers = value;
        }

        public List<UserInfo> Users
        {
            get => UserManager.Users;
            set => UserManager.Users = value;
        }

        private ConcurrentDictionary<long, BakiConfig> _groupBakiConfig = new ConcurrentDictionary<long, BakiConfig>();
        public ConcurrentDictionary<long, BakiConfig> GroupBakiConfig
        {
            get => _groupBakiConfig;
            set
            {
                if (value == null)
                {
                    _groupBakiConfig = new ConcurrentDictionary<long, BakiConfig>();
                }
                else
                {
                    _groupBakiConfig = value;
                }
            }
        }

        public static void Init()
        {
            var path = Config.ConfigFile;
            PlatformManager.Log.Debug( "InitPath:" + path);
            if (File.Exists(path))
            {
                var xml = File.ReadAllText(path);
                try
                {
                    var db = Tools.DeserializeObject<DataManager>(xml);
                    Instance = db;
                }
                catch(Exception e)
                {
                    PlatformManager.Log.Error("ConfigLoadError:" + e.Message);
                }
            }
            else
            {
                PlatformManager.Log.Debug("not found" + path);
            }

            if (Instance == null)
            {
                Instance = new DataManager();
            }

            _source = new CancellationTokenSource();
            SaveTask = Task.Factory.StartNew(() =>
            {
                while (!_source.IsCancellationRequested)
                {
                    Save();
                    Thread.Sleep(10 * 60 * 1000);
                }
            });
        }

        private static CancellationTokenSource _source;
        private static Task SaveTask;

        public static void Save()
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
                PlatformManager.Log.Debug("Config updated");
            }
            catch (Exception e)
            {
                PlatformManager.Log.Error(e.Message + e.StackTrace);
            }
        }

        public static void Stop()
        {
            _source.Cancel();
            Save();
        }

        public static DataManager Instance { get; set; }

    }
}
