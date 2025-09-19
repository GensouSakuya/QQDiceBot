using GensouSakuya.QQBot.Core.Base;
using GensouSakuya.QQBot.Core.Interfaces;
using GensouSakuya.QQBot.Core.Model;
using GensouSakuya.QQBot.Core.PlatformModel;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GensouSakuya.QQBot.Core.Handlers.Base
{
    internal abstract class BaseSubscribeHandler : IMessageCommandHandler, IBackgroundSubscribeHandler
    {
        protected readonly DataManager DataManager;
        protected readonly ILogger Logger;
        protected virtual TimeSpan StartDelay { get; } = TimeSpan.FromSeconds(5);
        protected virtual TimeSpan LoopInterval { get; } = TimeSpan.FromMinutes(1);
        protected TaskCompletionSource<bool> CompletionSource { get; set; }
        private CancellationTokenSource _loopCancellationTokenSource;
        protected Func<ConcurrentDictionary<string, ConcurrentDictionary<string, SubscribeModel>>> GetSubscribers { get; }

        public BaseSubscribeHandler(ILogger logger, DataManager dataManager, Func<ConcurrentDictionary<string, ConcurrentDictionary<string, SubscribeModel>>> getSubscribers)
        {
            Logger = logger;
            DataManager = dataManager;
            GetSubscribers = getSubscribers;
            CompletionSource = new TaskCompletionSource<bool>();
            _loopCancellationTokenSource = new CancellationTokenSource();
            _ = LoopCheck(_loopCancellationTokenSource.Token);
        }

        private async Task LoopCheck(CancellationToken token)
        {
            if (GetSubscribers == null)
                return;
            try
            {
                await Task.WhenAny(Task.Delay(StartDelay), CompletionSource.Task);
                Logger.LogInformation("{0} loop start", this.GetType().Name);
                while (!token.IsCancellationRequested)
                {
                    if (CompletionSource.Task.IsCompleted)
                        CompletionSource = new TaskCompletionSource<bool>();

                    var subscribers = GetSubscribers();
                    if(subscribers != null && subscribers.Count > 0)
                        await Loop(subscribers, token);

                    await Task.WhenAny(Task.Delay(LoopInterval), CompletionSource.Task);
                }
                Logger.LogError("loop finished");
            }
            catch (Exception e)
            {
                Logger.LogError(e, "bililive loop error");
            }
        }

        public abstract Task Loop(ConcurrentDictionary<string, ConcurrentDictionary<string, SubscribeModel>> subscribers, CancellationToken token);

        public virtual async Task<bool> ExecuteAsync(MessageSource source, IEnumerable<string> commandArgs, List<BaseMessage> originMessage, SourceFullInfo sourceInfo)
        {
            await Task.Yield();
            if (GetSubscribers == null)
            {
                return false;
            }
            SubscribeModel sbm;
            if (source.Type == MessageSourceType.Group)
            {
                if (sourceInfo.GroupMember.QQ != DataManager.Config.AdminQQ)
                {
                    MessageManager.SendToSource(source, "目前只有机器人管理员可以配置该功能哦");
                    return false;
                }

                sbm = new SubscribeModel
                {
                    Source = MessageSourceType.Group,
                    SourceId = source.GroupId
                };
            }
            else if (source.Type == MessageSourceType.Guild)
            {
                if (sourceInfo.GuildMember.UserId != DataManager.Config.AdminGuildUserId)
                {
                    MessageManager.SendToSource(source, "目前只有机器人管理员可以配置该功能哦");
                    return false;
                }

                sbm = new SubscribeModel
                {
                    Source = MessageSourceType.Guild,
                    SourceId = $"{source.GuildId}+{source.ChannelId}"
                };
            }
            else if (source.Type == MessageSourceType.Friend)
            {
                if (source.QQ != DataManager.Config.AdminQQ.ToString())
                {
                    MessageManager.SendToSource(source, "目前只有机器人管理员可以配置该功能哦");
                    return false;
                }

                sbm = new SubscribeModel
                {
                    Source = MessageSourceType.Friend,
                    SourceId = source.QQ
                };
            }
            else
            {
                MessageManager.SendToSource(source, "懒得支持！");
                return false;
            }

            if (!commandArgs.Any())
            {
                return false;
            }

            var first = commandArgs.ElementAt(0);
            if (first == "trigger")
            {
                CompletionSource.TrySetResult(true);
            }
            else
            {
                if (commandArgs.Count() < 2)
                {
                    return true;
                }
                var subscriber = GetSubscribers();
                var roomId = commandArgs.ElementAt(1);
                if (first == "subscribe")
                {
                    var sub = subscriber.GetOrAdd(roomId, new ConcurrentDictionary<string, SubscribeModel>());
                    if (sub.ContainsKey(sbm.ToString()))
                    {
                        MessageManager.SendToSource(source, "订阅已存在");
                        return false;
                    }

                    sub[sbm.ToString()] = sbm;
                    MessageManager.SendToSource(source, "订阅成功！");
                    DataManager.NoticeConfigUpdated();
                    return true;
                }
                else if (first == "unsubscribe")
                {
                    if (!subscriber.TryGetValue(roomId, out var sub))
                    {
                        return false;
                    }
                    if (sub.Remove(sbm.ToString(), out _))
                    {
                        MessageManager.SendToSource(source, "取消订阅成功！");
                        DataManager.NoticeConfigUpdated();
                    }
                    return true;
                }
            }

            return true;
        }
    }
}
