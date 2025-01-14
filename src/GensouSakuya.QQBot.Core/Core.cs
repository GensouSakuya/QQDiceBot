using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GensouSakuya.QQBot.Core.Base;
using GensouSakuya.QQBot.Core.Handlers;
using GensouSakuya.QQBot.Core.Interfaces;
using GensouSakuya.QQBot.Core.Model;
using GensouSakuya.QQBot.Core.PlatformModel;
using GensouSakuya.QQBot.Core.QQManager;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace GensouSakuya.QQBot.Core
{
    public class Core
    {
        private readonly IServiceCollection _serviceCollection;
        private ILogger _logger;
        public bool IsInitialized { get; private set; }

        private readonly HandlerResolver _handlerResolver;
        private IServiceProvider _messageServiceProvider;
        public Core()
        {
            _serviceCollection = new ServiceCollection();
            _serviceCollection.AddLogging(p =>
            {
                p.AddSerilog(new LoggerConfiguration()
                    .Enrich.FromLogContext()
                    .MinimumLevel.Debug()
                    .WriteTo.Console(
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                    .WriteTo.File(Path.Combine("logs", "qqbot-.log"), rollingInterval: RollingInterval.Day,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                        shared: true, rollOnFileSizeLimit: true, fileSizeLimitBytes: 10 * 1024 * 1024)
                    .CreateLogger());
            });
            _handlerResolver = new HandlerResolver();
        }

        public async Task Init(long qq, PlatformApiModel api, string dataPath = null)
        {
            await _handlerResolver.RegisterHandlers(_serviceCollection);
            CommandCenter.ReloadManagers();
            RegisterEvents(api);
            _messageServiceProvider = _serviceCollection.BuildServiceProvider();
            _logger = _messageServiceProvider.GetService<ILoggerFactory>().CreateLogger<Core>();
            await DataManager.Init(qq, dataPath);
            _logger.LogInformation("bot is started");
            IsInitialized = true;
        }

        public async Task HandlerMessage(MessageSource source, string rawMessage, List<BaseMessage> originMessage)
        {
            if (source == null)
                return;
            if (NeedIgnore(source))
                return;

            var commands = GetCommandFromMessage(rawMessage);
            var command = commands?.FirstOrDefault()?.ToLower();
            var commandArgs = commands?.Skip(1);
            var fullInfo = await FillSourceInfo(source);
            if (command == null) 
            {
                var chainHandlers = _handlerResolver.GetChainHandlers(_messageServiceProvider);
                await ExecuteChain(chainHandlers, source, rawMessage, originMessage, fullInfo);
                await CommandCenter.ExecuteWithoutCommand(source, rawMessage, originMessage, fullInfo);
            }
            else
            {
                var handler = _handlerResolver.GetCommandHandler(_messageServiceProvider, command);
                if(handler != null)
                {
                    await handler.ExecuteAsync(source, commandArgs, originMessage, fullInfo);
                }
                else
                {
                    await CommandCenter.Execute(source, rawMessage, command, commandArgs, originMessage, fullInfo);
                }
            }
        }

        private async Task<SourceFullInfo> FillSourceInfo(MessageSource source)
        {
            var sourceFullInfo = new SourceFullInfo();
            long? qq = source.QQ.HasValue() ? (long?)source.QQ.ToLong() : null;
            if (qq.HasValue)
            {
                if (source.Type != MessageSourceType.Guild)
                {
                    sourceFullInfo.QQ = await UserManager.Get(qq.Value);
                }
                else
                {
                    sourceFullInfo.GuildUser = await GuildUserManager.Get(qq.ToString(), source.GuildId);
                }
            }

            long? groupNo = source.GroupId.HasValue() ? (long?)source.GroupId.ToLong() : null;
            GuildMember guildMember = null;
            if (qq.HasValue)
            {
                if (groupNo.HasValue)
                {
                    var member = await GroupMemberManager.Get(qq.Value, groupNo.Value);
                    sourceFullInfo.GroupMember = member;
                }
                else if (source.GuildId.HasValue())
                {
                    guildMember = await GuildMemberManager.Get(qq.ToString(), source.GuildId);
                    if (guildMember != null)
                    {
                        sourceFullInfo.GuildMember = guildMember;
                        GuildMemberManager.UpdateNickName(guildMember, (string)source.Sender?.nickname);
                    }
                }
            }

            return sourceFullInfo;
        }

        private void RegisterEvents(PlatformApiModel api)
        {
            if (api == null)
                return;
            EventCenter.SendMessage = api.SendMessage;
            EventCenter.GetGroupMemberList = api.GetGroupMemberList;
        }

        private bool NeedIgnore(MessageSource source)
        {
            if(source.Type == MessageSourceType.Group)
            {
                if(source.GroupIdNum.Value.HasValue && source.QQNum.Value.HasValue 
                                                    && (DataManager.Instance?.GroupIgnore?.ContainsKey((source.GroupIdNum.Value.Value, source.QQNum.Value.Value)) ?? false))
                {
                    return true;
                }
            }

            return false;
        }

        private List<string> GetCommandFromMessage(string rawMessage)
        {
            if (!rawMessage.StartsWith(".") && !rawMessage.StartsWith("/"))
                return null;

            var commandStr = rawMessage.Remove(0, 1);
            var commandList = Tools.TakeCommandParts(commandStr);

            return commandList;
        }

        private async Task ExecuteChain(IEnumerable<IMessageChainHandler> handlers, MessageSource source, string message, List<BaseMessage> originMessage, SourceFullInfo sourceInfo)
        {
            try
            {
                if (handlers == null || !handlers.Any())
                    return;

                foreach (var handler in handlers)
                {
                    if (!await handler.Check(source, originMessage, sourceInfo))
                        continue;

                    var needContinue = await handler.NextAsync(source, originMessage, sourceInfo);

                    if (!needContinue)
                    {
                        break;
                    }
                }
                return;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "execute chain message error");
                return;
            }
        }
    }
}
