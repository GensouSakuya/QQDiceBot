using GensouSakuya.QQBot.Core.Model;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace GensouSakuya.QQBot.Core.Interfaces
{
    internal interface IBackgroundSubscribeHandler
    {
        Task Loop(ConcurrentDictionary<string, ConcurrentDictionary<string, SubscribeModel>> subscribers, CancellationToken token);
    }
}
