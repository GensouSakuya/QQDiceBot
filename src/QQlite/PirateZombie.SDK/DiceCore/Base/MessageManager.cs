using PirateZombie.SDK;
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
            QLAPI.Api_SendMsg(EventSourceType.Group.ToInt(), groupNo.ToString(), null, message, QLMain.ac);
        }

        public static void SendPrivate(long qq, string message)
        {
            QLAPI.Api_SendMsg(EventSourceType.Private.ToInt(), null, qq.ToString(), message, QLMain.ac);
        }
    }
}
