using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PirateZombie.SDK.BaseModel;

namespace net.gensousakuya.dice
{
    [Command("fudu")]
    public class RepeatManager : BaseManager
    {
        public override async Task ExecuteAsync(List<string> command, EventSourceType sourceType, UserInfo qq, Group group, GroupMember member)
        {
            var fromQQ = 0L;
            var toGroup = 0L;
            var message = "";
            if (sourceType != EventSourceType.Group)
            {
                return;
            }

            fromQQ = member.QQ;
            toGroup = member.GroupNumber;
            var permit = member.PermitType;
            if (!command.Any())
            {
                return;
            }

            if (command[0].Equals("on", StringComparison.CurrentCultureIgnoreCase))
            {
                if (permit == PermitType.None)
                {
                    MessageManager.Send(EventSourceType.Group, "只有群主或管理员才有权限开启复读功能", fromQQ, toGroup);
                    return;
                }
                RepeatConfig config;

                if (command.Count == 1)
                {
                    config = new RepeatConfig();
                }
                else
                {
                    if (int.TryParse(command[1], out var percent))
                    {
                        config = new RepeatConfig
                        {
                            Percent = percent
                        };
                    }
                    else
                    {
                        config = new RepeatConfig();
                    }
                }

                DataManager.Instance.GroupRepeatConfig.AddOrUpdate(toGroup, config, (p, q) => config);

                MessageManager.Send(EventSourceType.Group, $"复读已开启，复读概率：{config.Percent}%", fromQQ, toGroup);
            }
            else if (command[0].Equals("off", StringComparison.CurrentCultureIgnoreCase))
            {
                if (permit == PermitType.None)
                {
                    MessageManager.Send(EventSourceType.Group, "只有群主或管理员才有权限关闭复读功能", fromQQ, toGroup);
                    return;
                }

                DataManager.Instance.GroupRepeatConfig.TryRemove(toGroup, out _);
                MessageManager.Send(EventSourceType.Group, "复读已关闭", fromQQ, toGroup);
            }
            else if (command[0].Equals("repeat", StringComparison.CurrentCultureIgnoreCase) && command.Count>1)
            {
                MessageManager.Send(EventSourceType.Group, command[1], fromQQ, toGroup);
            }
        }
    }

    public class RepeatConfig
    {
        public int Percent { get; set; } = 5;
    }
}
