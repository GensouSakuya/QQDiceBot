
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
        public List<BaseMessage> Content { get; } = new List<BaseMessage>();

        public void AddTextMessage(string text)
        {
            Content.Add(new TextMessage(text));
        }

        public void AddImageMessage(string imagePath)
        {
            Content.Add(new ImageMessage(imagePath));
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
        public string Id{ get; set; }
        public ImageMessage(string path = null,string url = null,string id=null)
        {
            ImagePath = path;
            Url = url;
            Id = id;
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

    public class OtherMessage : BaseMessage
    {
        public object Origin{ get; set; }

        public OtherMessage(object origin)
        {
            Origin = origin;
        }
    }
}
