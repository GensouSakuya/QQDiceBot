using System.Collections.Generic;

namespace net.gensousakuya.dice
{
    //暂时写在代码里，后面再改成可配置
    public static class Config
    {
        public readonly static Dictionary<string, string> GroupCommandDesc = new Dictionary<string, string>
        {
            {
                ".ask [需要决策的问题] [供选择的方案(用'|'分隔)]", "向小夜征求意见"
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
            {
                ".ti", "疯狂发作-临时症状"
            }
        };

        public readonly static Dictionary<string, string> PrivateCommandDesc = new Dictionary<string, string>
        {
            {
                ".ask [需要决策的问题] [供选择的方案(用'|'分隔)]", "向小夜征求意见"
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
            {
                ".ti", "疯狂发作-临时症状"
            }
        };


    }
}
