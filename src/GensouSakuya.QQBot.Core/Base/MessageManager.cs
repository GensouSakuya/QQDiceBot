using GensouSakuya.QQBot.Core.PlatformModel;
using net.gensousakuya.dice;

namespace GensouSakuya.QQBot.Core.Base
{
    public class MessageManager
    {
        public static void SendTextMessage(MessageSourceType sourceType,string message,long? qq = null, long? toGroupNo = null)
        {
            switch (sourceType)
            {
                case MessageSourceType.Group:
                    if (!toGroupNo.HasValue || toGroupNo <= 0)
                        return;
                    SendTextMessageToGroup(toGroupNo.Value, message);
                    break;
                case MessageSourceType.Private:
                    if (!qq.HasValue || qq <= 0)
                        return;
                    SendTextMessagePrivate(qq.Value, message);
                    break;
                case MessageSourceType.Friend:
                    if (!qq.HasValue || qq <= 0)
                        return;
                    SendTextMessagePrivate(qq.Value, message);
                    break;
            }
        }

        public static void SendTextMessageToGroup(long groupNo,string textMessage)
        {
            var message = new Message();
            message.Type = MessageSourceType.Group;
            message.ToGroup = groupNo;
            message.AddTextMessage(textMessage);
            PlatformManager.SendMessage(message);
        }

        public static void SendTextMessagePrivate(long qq, string textMessage)
        {
            var message = new Message();
            message.Type = MessageSourceType.Private;
            message.ToQQ = qq;
            message.AddTextMessage(textMessage);
            PlatformManager.SendMessage(message);
        }

        public static void SendTextMessageFriend(long qq, string textMessage)
        {
            var message = new Message();
            message.Type = MessageSourceType.Friend;
            message.ToQQ = qq;
            message.AddTextMessage(textMessage);
            PlatformManager.SendMessage(message);
        }

        public static void SendImageMessage(MessageSourceType sourceType, string path, long? qq = null, long? toGroupNo = null)
        {
            var message = new Message();
            switch (sourceType)
            {
                case MessageSourceType.Group:
                    if (!toGroupNo.HasValue || toGroupNo <= 0)
                        return;
                    message.Type = MessageSourceType.Group;
                    message.ToGroup = toGroupNo.Value;
                    break;
                case MessageSourceType.Private:
                    if (!qq.HasValue || qq <= 0)
                        return;
                    message.Type = MessageSourceType.Private;
                    message.ToQQ = qq.Value;
                    SendTextMessagePrivate(qq.Value, path);
                    break;
                case MessageSourceType.Friend:
                    if (!qq.HasValue || qq <= 0)
                        return;
                    message.Type = MessageSourceType.Friend;
                    message.ToQQ = qq.Value;
                    break;
            }
            message.AddImageMessage(path);
            PlatformManager.SendMessage(message);
        }
    }
}
