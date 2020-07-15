using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PirateZombie.SDK;

namespace net.gensousakuya.dice
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

        public static List<GroupMember> GroupMembers
        {
            get => GroupMemberManager.GroupMembers;
            set => GroupMemberManager.GroupMembers = value;
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
            QLAPI.Api_SendLog("Debug", "InitPath:"+path, 0, QLMain.ac);
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
                    QLAPI.Api_SendLog("Error", "ConfigLoadError:"+e.Message, 0, QLMain.ac);
                }
            }
            else
            {
                QLAPI.Api_SendLog("Debug", "not found" + path, 0, QLMain.ac);
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
                QLAPI.Api_SendLog("Debug", "Config updated", 0, QLMain.ac);
            }
            catch (Exception e)
            {
                QLAPI.Api_SendLog("Error", e.Message+e.StackTrace, 0, QLMain.ac);
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
