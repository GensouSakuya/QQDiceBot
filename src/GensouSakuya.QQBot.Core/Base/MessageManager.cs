using net.gensousakuya.dice;

namespace GensouSakuya.QQBot.Core.Base
{
    public class MessageManager
    {
        public static void Send(MessageSourceType sourceType,string message,long? qq = null, long? toGroupNo = null)
        {
            switch (sourceType)
            {
                case MessageSourceType.Group:
                    if (!toGroupNo.HasValue || toGroupNo <= 0)
                        return;
                    SendToGroup(toGroupNo.Value, message);
                    break;
                case MessageSourceType.Private:
                    if (!qq.HasValue || qq <= 0)
                        return;
                    SendPrivate(qq.Value, message);
                    break;
                case MessageSourceType.Friend:
                    if (!qq.HasValue || qq <= 0)
                        return;
                    SendPrivate(qq.Value, message);
                    break;
            }
        }

        public static void SendToGroup(long groupNo,string message)
        {
            PlatformManager.SendGroupMessage(groupNo.ToString(), message);
        }

        public static void SendPrivate(long qq, string message)
        {
            PlatformManager.SendToNotFriend(qq.ToString(), message);
        }
        public static void SendFriend(long qq, string message)
        {
            PlatformManager.SendToFriend(qq.ToString(), message);
        }
    }
}
