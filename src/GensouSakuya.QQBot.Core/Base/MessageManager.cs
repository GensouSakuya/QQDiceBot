using System.Collections.Generic;
using GensouSakuya.QQBot.Core.Model;
using GensouSakuya.QQBot.Core.PlatformModel;

namespace GensouSakuya.QQBot.Core.Base
{
    public class MessageManager
    {
        public static void SendTextMessage(MessageSourceType sourceType,string message,long? qq = null, long? toGroupNo = null, string guildId = null, string channelId = null)
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
                    SendTextMessageFriend(qq.Value, message);
                    break;
                case MessageSourceType.Guild:
                    if(guildId.HasValue()&& channelId.HasValue())
                    {
                        SendTextMessageGuild(guildId,channelId,message);
                        break;
                    }
                    break;
            }
        }

        public static void SendToSource(MessageSource source, List<BaseMessage> messages)
        {
            var message = new Message();
            message.FromSource(source);
            message.Content.AddRange(messages);
            PlatformManager.SendMessage(message);
        }

        public static void SendToSource(MessageSource source, string text)
        {
            var message = new Message();
            message.FromSource(source);
            message.AddTextMessage(text);
            PlatformManager.SendMessage(message);
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

        public static void SendTextMessageGuild(string guildId, string channelId, string textMessage)
        {
            var message = new Message();
            message.Type = MessageSourceType.Guild;
            message.ToGuild = guildId;
            message.ToChannel = channelId;
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
