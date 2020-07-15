using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Native.Csharp.App;

namespace net.gensousakuya.dice
{
    [Command("shadiao")]
    public class ShaDiaoTuManager : BaseManager
    {
        private static Random _rand = new Random();
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
                if (permit == Native.Csharp.Sdk.Cqp.Enum.PermitType.None)
                {
                    MessageManager.Send(EventSourceType.Group, "只有群主或管理员才有权限开启沙雕图功能", fromQQ, toGroup);
                    return;
                }
                ShaDiaoTuConfig config;

                if (command.Count == 1)
                {
                    config = new ShaDiaoTuConfig();
                }
                else
                {
                    if (int.TryParse(command[1], out var percent))
                    {
                        config = new ShaDiaoTuConfig
                        {
                            Percent = percent
                        };
                    }
                    else
                    {
                        config = new ShaDiaoTuConfig();
                    }
                }

                DataManager.Instance.GroupShaDiaoTuConfig.AddOrUpdate(toGroup, config, (p, q) => config);
                MessageManager.Send(EventSourceType.Group, "随机沙雕图已开启", fromQQ, toGroup);
            }
            else if (command[0].Equals("off", StringComparison.CurrentCultureIgnoreCase))
            {
                if (permit == Native.Csharp.Sdk.Cqp.Enum.PermitType.None)
                {
                    MessageManager.Send(EventSourceType.Group, "只有群主或管理员才有权限关闭沙雕图功能", fromQQ, toGroup);
                    return;
                }

                DataManager.Instance.GroupShaDiaoTuConfig.TryRemove(toGroup, out _);
                MessageManager.Send(EventSourceType.Group, "随机沙雕图已关闭", fromQQ, toGroup);
            }
            else if (command[0].Equals("shadiaotu", StringComparison.CurrentCultureIgnoreCase) && command.Count > 1)
            {
                var dir = Path.Combine(Common.DataDirectory, "Shadiao");
                if (!Directory.Exists(dir))
                    return;
                var files = Directory.GetFiles(dir);
                if (!files.Any())
                    return;
                var fileName = files[_rand.Next(0, files.Length)];
                MessageManager.Send(sourceType, $"[CQ:image,file={fileName}]",
                    fromQQ, toGroup);
            }
        }
    }

    public class ShaDiaoTuConfig
    {
        public int Percent { get; set; }
    }
}
