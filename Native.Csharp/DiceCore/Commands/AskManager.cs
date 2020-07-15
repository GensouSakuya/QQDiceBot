using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace net.gensousakuya.dice
{
    [Command("ask")]
    public class AskManager : BaseManager
    {
        public override async Task ExecuteAsync(List<string> command, EventSourceType sourceType, UserInfo qq, Group group, GroupMember member)
        {
            if (command.Count < 1)
            {
                MessageManager.Send(sourceType, "不提问怎么帮你选0 0？", qq?.QQ, member?.GroupNumber);
                return;
            }

            if (command.Count < 2)
            {
                MessageManager.Send(sourceType, "快把你打算的选择告诉我！", qq?.QQ, member?.GroupNumber);
                return;
            }
            var quest = command.First();

            command.RemoveAt(0);
            var ansStr = string.Join(" ", command);
            var ans = ansStr.Split('|').ToList();
            var res = Ask(ans);
            var message = $"关于[{quest}]：\n" + string.Join("\n", res.Select(p => $"{p.Quest}:{p.Percent}%")) + "\n"
                          + $"小夜觉得{string.Join("、", GetMax(res).Select(p => p.Quest))}比较好";
            MessageManager.Send(sourceType, message, qq?.QQ, member?.GroupNumber);
        }

        public static List<AskModel> Ask(List<string> ques)
        {
            if (!ques.Any())
                return new List<AskModel>();
            var list = ques.Select(p => new AskModel
            {
                Quest = p
            }).ToList();
            int i = 0;
            while (i < 1000)
            {
                var dindex = DiceManager.RollDice(ques.Count) - 1;
                var des = DiceManager.RollDice(1000);
                if (des >= 850 && i <= 999)
                {
                    var newRes = DiceManager.RollDice(999 - i);
                    list[dindex].Percent += newRes / 10m;
                    i += newRes;
                }
                else
                {
                    list[dindex].Percent += 0.1m;
                    i++;
                }
            }
            return list;
        }

        public static List<AskModel> GetMax(List<AskModel> model)
        {
            var maxValue = model.Max(p => p.Percent);
            return model.Where(p => p.Percent == maxValue).ToList();
        }

        public class AskModel
        {
            public string Quest { get; set; }
            public decimal Percent { get; set; } = 0m;
        }
    }
}
