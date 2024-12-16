
using GensouSakuya.QQBot.Core.Model;
using System;
using System.Collections.Generic;

namespace GensouSakuya.QQBot.Core.PlatformModel
{
    public class Message
    {
        public MessageSourceType Type { get; set; }
        //public string FromQQ { get; set; }
        public long ToQQ { get; set; }
        public long ToGroup { get; set; }
        public string ToGuild { get; set; }
        public string ToChannel { get; set; }
        public List<BaseMessage> Content { get; } = new List<BaseMessage>();

        public void AddTextMessage(string text)
        {
            Content.Add(new TextMessage(text));
        }

        public void AddImageMessage(string imagePath)
        {
            Content.Add(new ImageMessage(imagePath));
        }

        public void AddRange(List<BaseMessage> messages)
        {
            Content.AddRange(messages);
        }

        public void FromSource(MessageSource source)
        {
            Type = source.Type;
            switch (source.Type)
            {
                case MessageSourceType.Group:
                    ToGroup = source.GroupId.ToLong();
                    break;
                case MessageSourceType.Private:
                    ToQQ = source.QQ.ToLong();
                    break;
                case MessageSourceType.Friend:
                    ToQQ = source.QQ.ToLong();
                    break;
                case MessageSourceType.Guild:
                    ToGuild = source.GuildId;
                    ToChannel = source.ChannelId;
                    break;
            }
        }
    }

    public abstract class BaseMessage
    {

    }

    public class TextMessage: BaseMessage
    {
        public string Text { get; set; }

        public TextMessage(string text)
        {
            Text = text;
        }
    }

    public class ImageMessage : BaseMessage
    {
        /// <summary>
        /// 发送用
        /// </summary>
        public string ImagePath { get; set; }

        /// <summary>
        /// 接收用
        /// </summary>
        public string Url{ get; set; }

        /// <summary>
        /// 接收用
        /// </summary>
        public string ImageId{ get; set; }

        /// <summary>
        /// 是否是临时文件，如果不是。那么发送时拷贝到临时目录再进行发送
        /// </summary>
        public bool IsTempImg { get; set; }

        public ImageMessage(string path = null,string url = null,string imageId=null, bool isTemp = true)
        {
            ImagePath = path;
            Url = url;
            ImageId = imageId;
            IsTempImg = isTemp;
        }
    }

    public class AtMessage : BaseMessage
    {
        public long QQ{ get; set; }

        public AtMessage(long qq)
        {
            QQ = qq;
        }
    }

    public class QuoteMessage : BaseMessage
    {
        public long GroupNumber { get; set; }

        public long SenderId { get; set; }

        public long MessageId { get; set; }

        public QuoteMessage(long? groupNumber,long? senderId,long? messageId)
        {
            GroupNumber = groupNumber ?? default;
            SenderId = senderId ?? default;
            MessageId = messageId ?? default;
        }
    }

    public class OtherMessage : BaseMessage
    {
        public object Origin{ get; set; }

        public OtherMessage(object origin)
        {
            Origin = origin;
        }
    }

    public class VoiceMessage : BaseMessage
    {
        /// <summary>
        /// 接收用
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 接收用
        /// </summary>
        public string VoiceId { get; set; }

        public VoiceMessage(string url = null, string voiceId = null)
        {
            Url = url;
            VoiceId = voiceId;
        }
    }

    public class SourceMessage : BaseMessage
    {
        public int Id { get; set; }

        public DateTime Time { get; set; }

        public SourceMessage(int id, DateTime time)
        {
            this.Id = id;
            this.Time = time;
        }
    }

    public class JsonMessage: BaseMessage
    {
        public string Json { get; set; }

        public JsonMessage(string json)
        {
            Json = json;
        }
    }
}
