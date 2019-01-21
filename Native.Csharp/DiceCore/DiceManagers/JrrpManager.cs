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

        public static int GetJrrp(UserInfo user)
        {
            //LastJrrpDate为空即表示第一次使用jrrp，不做特殊处理
            if (!user.LastJrrpDate.HasValue)
            {
                user.Jrrp = DiceManager.RollDice();
                user.LastJrrpDate = DateTime.Today;
                return user.Jrrp;
            }
            else
            {
                //LastJrrpDate有值但不为今日则表明过去使用过jrrp而今日未使用，需要重新计算jrrp值
                if (user.LastJrrpDate != DateTime.Today)
                {
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
                    }
                    else
                    {
                        user.JrrpDurationDays = 0;

                        user.Jrrp = DiceManager.RollDice();
                    }
                    user.LastJrrpDate = DateTime.Today;
                }
                return user.Jrrp;
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

            var rp = GetJrrp(qq);

            if (rp == -1)
            {
                MessageManager.Send(sourceType, name + "迷信小夜不可取！不给你看今天的结果w(ﾟДﾟ)w", qq: qq?.QQ, toGroupNo: member?.GroupNumber);
            }
            else
            {
                MessageManager.Send(sourceType, name + "今天的人品值是:" + rp, qq: qq?.QQ, toGroupNo: member?.GroupNumber);
            }
        }
    }
}
