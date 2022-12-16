namespace GensouSakuya.QQBot.Core.PlatformModel
{
    public enum MessageSourceType
    {
        Discuss = 1,
        Friend = 2,
        Group = 3,
        Private = 4,
        Guild = 5
    }

    //public static class SourceExtension
    //{
    //    public static int ToInt(this MessageSourceType source)
    //    {
    //        //* type：1.好友消息 2.群消息 3.群临时消息 4.讨论组消息 5.讨论组临时消息 6.QQ临时消息
    //        switch (source)
    //        {
    //            case MessageSourceType.Discuss:
    //                return 4;
    //            case MessageSourceType.Friend:
    //                return 1;
    //            case MessageSourceType.Group:
    //                return 2;
    //            case MessageSourceType.Private:
    //                return 6;
    //        }

    //        return 0;
    //    }
    //}
}
