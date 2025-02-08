using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GensouSakuya.QQBot.Core.Base;
using GensouSakuya.QQBot.Core.Interfaces;
using GensouSakuya.QQBot.Core.Model;
using GensouSakuya.QQBot.Core.PlatformModel;
using Microsoft.Extensions.Logging;
using net.gensousakuya.dice;

namespace GensouSakuya.QQBot.Core.Commands
{
    internal class JrrpHandler : IMessageCommandHandler
    {
        private readonly DataManager _dataManager;
        private readonly ILogger _logger;
        public JrrpHandler(DataManager data, ILoggerFactory loggerFactory)
        {
            _dataManager = data;
            _logger = loggerFactory.CreateLogger<JrrpHandler>();
        }

        private List<long> _disabledJrrpGroupNumbers
        {
            get { return _dataManager.Config.DisabledJrrpGroupNumbers; }
        }

        internal int GetJrrp(IUserJrrp user, bool reroll = false)
        {
            //LastJrrpDate为空即表示第一次使用jrrp，不做特殊处理
            if (!user.LastJrrpDate.HasValue)
            {
                user.Jrrp = DiceManager.RollDice();
                user.LastJrrpDate = DateTime.Today;
                ReRollCheck(ref user);
                _dataManager.NoticeConfigUpdated();
                return user.Jrrp;
            }
            else
            {
                //LastJrrpDate有值但不为今日则表明过去使用过jrrp而今日未使用，需要重新计算jrrp值
                if (user.LastJrrpDate != DateTime.Today)
                {
                    user.ReRollStep = RerollStep.None;
                    user.Jrrp = DiceManager.RollDice();

                    ReRollCheck(ref user);
                    user.LastJrrpDate = DateTime.Today;
                    _dataManager.NoticeConfigUpdated();
                }
                else
                {
                    if (reroll)
                    {
                        var rerollJrrp = DiceManager.RollDice();
                        if (rerollJrrp > 70)
                        {
                            user.ReRollStep = RerollStep.RerollSuccess;
                            user.Jrrp = rerollJrrp;
                        }
                        else if (rerollJrrp > 25)
                        {
                            user.ReRollStep = RerollStep.RerollFaild;
                        }
                        else
                        {
                            user.ReRollStep = RerollStep.RerollDevastated;
                            user.Jrrp = DiceManager.RollDice((rerollJrrp + 1) / 2);
                        }
                        _dataManager.NoticeConfigUpdated();
                    }
                }
                return user.Jrrp;
            }
        }

        internal static void ReRollCheck(ref IUserJrrp user)
        {
            if (user.Jrrp > 0 && user.Jrrp <= 25)
            {
                //jrrp在(0,25]时可以reroll一次
                user.ReRollStep = RerollStep.CanReroll;
            }
        }

        public async Task<bool> ExecuteAsync(MessageSource source, IEnumerable<string> command, List<BaseMessage> originMessage, SourceFullInfo sourceInfo)
        {
            await Task.Yield();
            var name = "";
            IUserJrrp user = null;
            var qq = sourceInfo.QQ;
            if (source.Type == MessageSourceType.Group)
            {
                var member = sourceInfo.GroupMember;
                if (member == null)
                {
                    _logger.LogDebug("Jrrp group member is null");
                    return false;
                }
                if (_disabledJrrpGroupNumbers.Contains(member.GroupNumber))
                {
                    return false;
                }

                name = member.GroupName;
                user = qq;
            }
            else if (source.Type == MessageSourceType.Private || source.Type == MessageSourceType.Friend)
            {
                if (qq == null)
                {
                    _logger.LogDebug("Jrrp qq is null");
                    return false;
                }
                name = qq.Name;
                user = qq;
            }
            else if (source.Type == MessageSourceType.Guild)
            {
                user = sourceInfo.GuildUser;
                name = sourceInfo.GuildMember.NickName;
            }
            else
            {
                throw new NotImplementedException("not support type");
            }

            var isReroll = user.ReRollStep == RerollStep.CanReroll;
            var rp = GetJrrp(user, isReroll);

            switch (user.ReRollStep)
            {
                case RerollStep.None:
                    MessageManager.SendToSource(source, name + "今天的人品值是:" + rp);
                    break;
                case RerollStep.CanReroll:
                    MessageManager.SendToSource(source, name + "今天的人品不太好，确定要看的话就再来一次吧");
                    break;
                case RerollStep.RerollFaild:
                    if (rp == 1)
                    {
                        MessageManager.SendToSource(source, $"……");
                    }
                    else
                    {
                        MessageManager.SendToSource(source, name + $"今天的人品值只有：{rp}");
                    }
                    break;
                case RerollStep.RerollSuccess:
                    MessageManager.SendToSource(source, $"啊！对不起刚才是我失误了！{name}今天人品值应该是：{rp}");
                    user.ReRollStep = RerollStep.None; 
                    break;
                case RerollStep.RerollDevastated:
                    if (rp == 1)
                    {
                        MessageManager.SendToSource(source, $"……");
                    }
                    else
                    {
                        MessageManager.SendToSource(source, $"都说了不想告诉你了嘛……{name}今天人品值只有：{rp}");
                    }
                    user.ReRollStep = RerollStep.RerollFaild;
                    break;
            }
            return true;
        }
    }
}
