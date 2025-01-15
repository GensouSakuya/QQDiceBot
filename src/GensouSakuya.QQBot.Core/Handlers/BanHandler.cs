using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GensouSakuya.QQBot.Core.Base;
using GensouSakuya.QQBot.Core.Interfaces;
using GensouSakuya.QQBot.Core.Model;
using GensouSakuya.QQBot.Core.PlatformModel;

namespace GensouSakuya.QQBot.Core.Commands
{
    internal class BanHandler : IMessageCommandHandler
    {
        private readonly DataManager _dataManager;
        public BanHandler(DataManager data)
        {
            _dataManager = data;
        }

        public Task<bool> ExecuteAsync(MessageSource source, IEnumerable<string> command, List<BaseMessage> originMessage, SourceFullInfo sourceInfo)
        {
            if (source.Type != MessageSourceType.Group)
            {
                return Task.FromResult(false);
            }

            var member = sourceInfo.GroupMember;
            var permit = member.PermitType;
            if (permit == PermitType.None)
            {
                MessageManager.SendToSource(source, "只有群主或管理员才有权限封禁用户");
                return Task.FromResult(false);
            }

            if (!command.Any())
                return Task.FromResult(false);

            if (!long.TryParse(command.ElementAt(0), out var banQQ))
            {
                return Task.FromResult(false);
            }

            if (command.Count() > 1 && long.TryParse(command.ElementAt(1), out var banGroup))
            {
                var groupBan = _dataManager.Config.GroupBan;
                if (groupBan.ContainsKey((banGroup, banQQ)))
                {
                    groupBan.TryRemove((banGroup, banQQ), out _);
                    MessageManager.SendToSource(source, $"用户{banQQ}在群{banGroup}的封禁已被解除");
                }
                else
                {
                    groupBan.TryAdd((banGroup, banQQ), null);
                    MessageManager.SendToSource(source, $"用户{banQQ}在群{banGroup}已被封禁");
                }
            }
            else
            {
                var qqBan = _dataManager.Config.QQBan;
                if (qqBan.ContainsKey(banQQ))
                {
                    qqBan.TryRemove(banQQ, out _);
                    MessageManager.SendToSource(source, $"用户{banQQ}的全局封禁已被解除");
                }
                else
                {
                    qqBan.TryAdd(banQQ, null);
                    MessageManager.SendToSource(source, $"用户{banQQ}已被全局封禁");
                }
            }
            _dataManager.NoticeConfigUpdated();
            return Task.FromResult(true);
        }
    }
}
