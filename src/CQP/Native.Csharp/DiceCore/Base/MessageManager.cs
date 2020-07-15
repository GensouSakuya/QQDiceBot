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
                case EventSourceType.Private:
                    if (!qq.HasValue || qq <= 0)
                        return;
                    SendPrivate(qq.Value, message);
                    break;
            }
        }

        public static void SendToGroup(long groupNo,string message)
        {
            Common.CqApi.SendGroupMessage(groupNo, message); //Common.CqApi.CqCode_At(e.FromQQ) + "你发送了这样的消息: " + e.Msg);
        }

        public static void SendPrivate(long qq, string message)
        {
            Common.CqApi.SendPrivateMessage(qq, message);
        }
    }
}
