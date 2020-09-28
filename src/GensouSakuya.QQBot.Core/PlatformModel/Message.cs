
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
        public string ImagePath { get; set; }
        public ImageMessage(string path)
        {
            ImagePath = path;
        }
    }
}
