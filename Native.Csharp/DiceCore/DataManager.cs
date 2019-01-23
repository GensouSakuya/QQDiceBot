using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace net.gensousakuya.dice
{
    public class DataManager
    {
        private static DataManager _instance { get; set; } = new DataManager();
        private static volatile object locker = new object();

        private DataManager()
        { }
        
        public long AdminQQ { get; set; }
        public List<UserInfo> Users { get; set; } = new List<UserInfo>();
        public List<Group> Groups { get; set; } = new List<Group>();
        public List<GroupMember> GroupMember { get; set; } = new List<GroupMember>();

        public List<long> DisabledJrrpGroupNumbers { get; set; } = new List<long>();

        private const string fileName = "DiceData";
        public static void Init(string path)
        {
            if (Directory.Exists(path))
            {
                var filepath = Path.Combine(path, fileName);
                if (File.Exists(filepath))
                {
                    var xml = File.ReadAllText(filepath);
                    try
                    {
                        var db = Tools.DeserializeObject<DataManager>(xml);
                        _instance = db;
                    }
                    catch { }
                }
            }
        }

        public static void Save(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            var filepath = Path.Combine(path, fileName);
            var xml = Tools.SerializeObject(_instance);
            File.WriteAllText(filepath, xml);
        }

        public static void Init(DataManager db)
        {
            _instance = db;
        }

        public static DataManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (locker)
                    {
                        if (_instance == null)
                        {
                            _instance = new DataManager();
                        }
                    }
                }

                return _instance;
            }
        }

    }
}
