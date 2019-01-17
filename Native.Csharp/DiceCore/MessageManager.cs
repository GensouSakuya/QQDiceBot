using Native.Csharp.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace net.gensousakuya.dice
{
    public class MessageManager
    {
        public static void Send(EventSourceType sourceType,string message,long? qq = null, long? toGroupNo = null)
        {
            switch (sourceType)
            {
                case EventSourceType.Group:
                    if (!toGroupNo.HasValue || toGroupNo <= 0)
                        return;
                    SendToGroup(toGroupNo.Value, message);
                    break;
            }
        }

        public static void SendToGroup(long groupNo,string Message)
        {
            Common.CqApi.SendGroupMessage(groupNo, Message); //Common.CqApi.CqCode_At(e.FromQQ) + "你发送了这样的消息: " + e.Msg);
        }
    }
}
