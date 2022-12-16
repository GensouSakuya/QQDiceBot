using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GensouSakuya.QQBot.Core.Base;
using GensouSakuya.QQBot.Core.Interfaces;
using GensouSakuya.QQBot.Core.Model;
using GensouSakuya.QQBot.Core.PlatformModel;
using net.gensousakuya.dice;

namespace GensouSakuya.QQBot.Core.Commands
{
    [Command("jrrp")]
    public class JrrpManager: BaseManager
    {
        private static readonly Logger _logger = Logger.GetLogger<JrrpManager>();
        private static List<long> _disabledJrrpGroupNumbers
        {
            get { return DataManager.Instance.DisabledJrrpGroupNumbers; }
        }

        internal static int GetJrrp(IUserJrrp user, bool reroll = false)
        {
            //LastJrrpDate为空即表示第一次使用jrrp，不做特殊处理
            if (!user.LastJrrpDate.HasValue)
            {
                user.Jrrp = DiceManager.RollDice();
                user.LastJrrpDate = DateTime.Today;
                ReRollCheck(ref user);
                DataManager.Instance.NoticeConfigUpdated();
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
                    DataManager.Instance.NoticeConfigUpdated();
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
                        DataManager.Instance.NoticeConfigUpdated();
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

        public override async Task ExecuteAsync(MessageSource source, List<string> command, List<BaseMessage> originMessage, UserInfo qq, Group group, GroupMember member, GuildUserInfo guildUser, GuildMember guildmember)
        {
            await Task.Yield();
            var name = "";
            IUserJrrp user = null;
            if (source.Type == MessageSourceType.Group)
            {
                if (member == null)
                {
                    _logger.Debug("Jrrp group member is null");
                    return;
                }
                if (_disabledJrrpGroupNumbers.Contains(member.GroupNumber))
                {
                    return;
                }

                name = member.GroupName;
                user = qq;
            }
            else if (source.Type == MessageSourceType.Private || source.Type == MessageSourceType.Friend)
            {
                if (qq == null)
                {
                    _logger.Debug("Jrrp qq is null");
                    return;
                }
                name = qq.Name;
                user = qq;
            }
            else if(source.Type == MessageSourceType.Guild)
            {
                user = guildUser;
                name = guildmember.NickName;
            }
            else
            {
                throw new NotImplementedException("not support type");
            }

            var isReroll = user.ReRollStep == RerollStep.CanReroll;
            var rp = GetJrrp(user, isReroll);

            switch(user.ReRollStep)
            {
                case RerollStep.None:
                    MessageManager.SendToSource(source, name + "今天的人品值是:" + rp);
                    return;
                case RerollStep.CanReroll:
                    MessageManager.SendToSource(source, name + "今天的人品不太好，确定要看的话就再来一次吧");
                    return;
                case RerollStep.RerollFaild:
                    if (rp == 1)
                    {
                        MessageManager.SendToSource(source, $"……");
                    }
                    else
                    {
                        MessageManager.SendToSource(source, name + $"今天的人品值只有：{rp}");
                    }
                    return;
                case RerollStep.RerollSuccess:
                    MessageManager.SendToSource(source, $"啊！对不起刚才是我失误了！{name}今天人品值应该是：{rp}");
                    user.ReRollStep = RerollStep.None;
                    return;
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
                    return;
            }
        }
    }
}
