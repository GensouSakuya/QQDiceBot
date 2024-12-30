using GensouSakuya.QQBot.Core.Model;
using GensouSakuya.QQBot.Core.PlatformModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GensouSakuya.QQBot.Core.Interfaces
{
    internal interface IMessageChainHandler: IMessageHandler
    {
        Task<bool> Check(MessageSource source, List<BaseMessage> originMessage, SourceFullInfo sourceInfo);

        Task<bool> NextAsync(MessageSource source, List<BaseMessage> originMessage, SourceFullInfo sourceInfo);
    }
}
