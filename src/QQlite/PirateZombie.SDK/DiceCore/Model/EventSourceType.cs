namespace net.gensousakuya.dice
{
    public enum EventSourceType
    {
        Discuss = 1,
        Friend = 2,
        Group = 3,
        Private = 4,
    }

    public static class SourceExtension
    {
        public static int ToInt(this EventSourceType source)
        {
            //* type：1.好友消息 2.群消息 3.群临时消息 4.讨论组消息 5.讨论组临时消息 6.QQ临时消息
            switch (source)
            {
                case EventSourceType.Discuss:
                    return 4;
                case EventSourceType.Friend:
                    return 1;
                case EventSourceType.Group:
                    return 2;
                case EventSourceType.Private:
                    return 6;
            }

            return 0;
        }
    }
}
