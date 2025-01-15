using GensouSakuya.QQBot.Core.Commands.V2;
using GensouSakuya.QQBot.Core.Commands;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace GensouSakuya.QQBot.Core.Model
{
    internal class ConfigData
    {
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
    }
}
