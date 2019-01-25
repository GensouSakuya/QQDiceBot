using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
                    var yesterday = DateTime.Today.AddDays(-1);
                    if (user.LastJrrpDate.Value == yesterday)
                    {
                        user.JrrpDurationDays++;

                        if (user.JrrpDurationDays > 1)
                        {
                            var dontShowRoll = DiceManager.RollDice();
                            var dontShowCheckMax = user.JrrpDurationDays > 11 ? 33 : user.JrrpDurationDays * 3;
                            if (dontShowRoll <= dontShowCheckMax)
                            {
                                user.Jrrp = -1;
                            }
                            else
                            {
                                user.Jrrp = DiceManager.RollDice();
                            }
                        }
                        else
                        {
                            user.Jrrp = DiceManager.RollDice();
                        }
                    }
                    else
                    {
                        user.JrrpDurationDays = 0;

                        user.Jrrp = DiceManager.RollDice();
                    }

                    ReRollCheck(ref user);
                    user.LastJrrpDate = DateTime.Today;
                }
                else
                {
                    if (reroll)
                    {
                        var rerollJrrp = DiceManager.RollDice();
                        if (rerollJrrp <= 70)
                        {
                            user.ReRollStep = UserInfo.RerollStep.RerollFaild;
                        }
                        else
                        {
                            user.ReRollStep = UserInfo.RerollStep.RerollSuccess;
                            user.Jrrp = rerollJrrp;
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

            if (rp == -1)
            {
                MessageManager.Send(sourceType, name + "迷信小夜不可取！不给你看今天的结果w(ﾟДﾟ)w", qq: qq?.QQ, toGroupNo: member?.GroupNumber);
            }
            else
            {
                switch(qq.ReRollStep)
                {
                    case UserInfo.RerollStep.None:
                        MessageManager.Send(sourceType, name + "今天的人品值是:" + rp, qq: qq?.QQ, toGroupNo: member?.GroupNumber);
                        return;
                    case UserInfo.RerollStep.CanReroll:
                        MessageManager.Send(sourceType, name + "今天的人品太惨了，确定要看今天的结果吗", qq: qq?.QQ, toGroupNo: member?.GroupNumber);
                        return;
                    case UserInfo.RerollStep.RerollFaild:
                        MessageManager.Send(sourceType, name + $"今天的人品太惨了，只有{rp}，而且试图改命还失败了", qq: qq?.QQ, toGroupNo: member?.GroupNumber);
                        return;
                    case UserInfo.RerollStep.RerollSuccess:
                        MessageManager.Send(sourceType, name + $"逆天改命成功了！今天人品值是：{rp}", qq: qq?.QQ, toGroupNo: member?.GroupNumber);
                        return;
                }
            }
        }
    }
}
