using GensouSakuya.QQBot.Core.Model;
using GensouSakuya.QQBot.Core.PlatformModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GensouSakuya.QQBot.Core.Interfaces
{
    public interface IMessageCommandHandler: IMessageHandler
    {
        Task<bool> ExecuteAsync(MessageSource source, IEnumerable<string> command, List<BaseMessage> originMessage, SourceFullInfo sourceInfo);
    }
}
