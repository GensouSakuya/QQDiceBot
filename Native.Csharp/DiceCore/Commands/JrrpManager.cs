using System;
using System.Collections.Generic;

namespace net.gensousakuya.dice
{
    public class JrrpManager: BaseManager
    {
        private static List<long> _disabledJrrpGroupNumbers
        {
            get { return DataManager.Instance.DisabledJrrpGroupNumbers; }
        }

        public static int GetJrrp(UserInfo user, bool reroll = false)
        {
            //LastJrrpDate为空即表示第一次使用jrrp，不做特殊处理
            if (!user.LastJrrpDate.HasValue)
            {
                user.Jrrp = DiceManager.RollDice();
                user.LastJrrpDate = DateTime.Today;
                ReRollCheck(ref user);
                return user.Jrrp;
            }
            else
            {
                //LastJrrpDate有值但不为今日则表明过去使用过jrrp而今日未使用，需要重新计算jrrp值
                if (user.LastJrrpDate != DateTime.Today)
                {
                    user.ReRollStep = UserInfo.RerollStep.None;
                    user.Jrrp = DiceManager.RollDice();

                    ReRollCheck(ref user);
                    user.LastJrrpDate = DateTime.Today;
                }
                else
                {
                    if (reroll)
                    {
                        var rerollJrrp = DiceManager.RollDice();
                        if (rerollJrrp > 70)
                        {
                            user.ReRollStep = UserInfo.RerollStep.RerollSuccess;
                            user.Jrrp = rerollJrrp;
                        }
                        else if (rerollJrrp > 25)
                        {
                            user.ReRollStep = UserInfo.RerollStep.RerollFaild;
                        }
                        else
                        {
                            user.ReRollStep = UserInfo.RerollStep.RerollDevastated;
                            user.Jrrp = DiceManager.RollDice((rerollJrrp + 1) / 2);
                        }
                    }
                }
                return user.Jrrp;
            }
        }

        public static void ReRollCheck(ref UserInfo user)
        {
            if (user.Jrrp > 0 && user.Jrrp <= 25)
            {
                //jrrp在(0,25]时可以reroll一次
                user.ReRollStep = UserInfo.RerollStep.CanReroll;
            }
        }

        public override void Execute(List<string> command, EventSourceType sourceType, UserInfo qq, Group group, GroupMember member)
        {
            var name = "";
            if (sourceType == EventSourceType.Group)
            {
                if (member == null)
                    return;
                if (_disabledJrrpGroupNumbers.Contains(member.GroupNumber))
                {
                    return;
                }

                name = string.IsNullOrWhiteSpace(member.GroupName) ? qq.Name : member.GroupName;
            }
            else if (sourceType == EventSourceType.Private)
            {
                if (qq == null)
                    return;
                name = qq.Name;
            }

            var isReroll = qq.ReRollStep == UserInfo.RerollStep.CanReroll;
            var rp = GetJrrp(qq, isReroll);

            switch(qq.ReRollStep)
            {
                case UserInfo.RerollStep.None:
                    MessageManager.Send(sourceType, name + "今天的人品值是:" + rp, qq: qq?.QQ, toGroupNo: member?.GroupNumber);
                    return;
                case UserInfo.RerollStep.CanReroll:
                    MessageManager.Send(sourceType, name + "今天的人品不太好，确定要看的话就再来一次吧", qq: qq?.QQ, toGroupNo: member?.GroupNumber);
                    return;
                case UserInfo.RerollStep.RerollFaild:
                    if (rp == 1)
                    {
                        MessageManager.Send(sourceType, $"……", qq: qq?.QQ, toGroupNo: member?.GroupNumber);
                    }
                    else
                    {
                        MessageManager.Send(sourceType, name + $"今天的人品值只有：{rp}", qq: qq?.QQ, toGroupNo: member?.GroupNumber);
                    }
                    return;
                case UserInfo.RerollStep.RerollSuccess:
                    MessageManager.Send(sourceType, $"啊！对不起刚才是我失误了！{name}今天人品值应该是：{rp}", qq: qq?.QQ, toGroupNo: member?.GroupNumber);
                    qq.ReRollStep = UserInfo.RerollStep.None;
                    return;
                case UserInfo.RerollStep.RerollDevastated:
                    if (rp == 1)
                    {
                        MessageManager.Send(sourceType, $"……", qq: qq?.QQ, toGroupNo: member?.GroupNumber);
                    }
                    else
                    {
                        MessageManager.Send(sourceType, $"都说了不想告诉你了嘛……{name}今天人品值只有：{rp}", qq: qq?.QQ, toGroupNo: member?.GroupNumber);
                    }
                    qq.ReRollStep = UserInfo.RerollStep.RerollFaild;
                    return;
            }
        }
    }
}
