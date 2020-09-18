using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GensouSakuya.QQBot.Core.Base
{
    //暂时写在代码里，后面再改成可配置
    public static class Config
    {
        public static Dictionary<string, string> GroupCommandDesc => new Dictionary<string, string>
        {
            {
                ".ask [需要决策的问题] [供选择的方案(用'|'分隔)]", $"向{DataManager.Instance.BotName}征求意见"
            },
            {
                ".jrrp", "今日人品检定"
            },
            {
                ".li", "疯狂发作-总结症状"
            },
            {
                ".me [需要转述的内容]", "转述"
            },
            {
                ".nn [昵称]", "设置/删除群内昵称"
            },
            {
                ".null", "略一下"
            },
            //{
            //    ".setu [Tags]","发一张随机图"
            //},
            {
                ".ti", "疯狂发作-临时症状"
            }
        };

        public static Dictionary<string, string> PrivateCommandDesc => new Dictionary<string, string>
        {
            {
                ".ask [需要决策的问题] [供选择的方案(用'|'分隔)]", $"向{DataManager.Instance.BotName}征求意见"
            },
            {
                ".jrrp", "今日人品检定"
            },
            {
                ".li", "疯狂发作-总结症状"
            },
            {
                ".me [转述群号] [需要转述的内容]", "转述"
            },
            {
                ".null", "略一下"
            },
            //{
            //    ".setu [Tags]","发一张随机图"
            //},
            {
                ".ti", "疯狂发作-临时症状"
            }
        };

        public static readonly string DataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),".QQBot", "net.gensousakuya.dice");
        public static readonly string ConfigFile = Path.Combine(DataPath, "config.json");
        public static readonly string LogPath = Path.Combine(DataPath, "Log");
        public static readonly string ShaDiaoImagePath = Path.Combine(DataPath, "沙雕图");

        public static readonly Encoding DefaultEncoding = Encoding.GetEncoding("GB18030");
    }
}
