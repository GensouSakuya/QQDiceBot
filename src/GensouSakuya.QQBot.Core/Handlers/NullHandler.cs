using System.Collections.Generic;
using System.Threading.Tasks;
using GensouSakuya.QQBot.Core.Base;
using GensouSakuya.QQBot.Core.Interfaces;
using GensouSakuya.QQBot.Core.Model;
using GensouSakuya.QQBot.Core.PlatformModel;

namespace GensouSakuya.QQBot.Core.Handlers
{
    [Help("略一下")]
    internal class NullHandler : IMessageCommandHandler
    {
        public Task<bool> ExecuteAsync(MessageSource source, IEnumerable<string> commandArgs, List<BaseMessage> originMessage, SourceFullInfo sourceInfo)
        {
            MessageManager.SendToSource(source, "略略略😝");
            return Task.FromResult(true);
        }
    }
}
