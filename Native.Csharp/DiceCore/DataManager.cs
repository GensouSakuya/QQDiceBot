using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace net.gensousakuya.dice
{
    public class DataManager
    {
        private static DataManager _instance { get; set; } = new DataManager();
        private static volatile object locker = new object();

        private DataManager()
        { }

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

        public List<UserInfo> Users { get; set; } = new List<UserInfo>();
        public List<Group> Groups { get; set; } = new List<Group>();
        public List<GroupMember> GroupMember { get; set; } = new List<GroupMember>();

        public List<long> DisabledJrrpGroupNumbers { get; set; } = new List<long>();
    }
}
