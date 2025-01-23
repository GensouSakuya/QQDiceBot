using GensouSakuya.QQBot.Core.Base;
using GensouSakuya.QQBot.Core.Interfaces;
using GensouSakuya.QQBot.Core.Model;
using GensouSakuya.QQBot.Core.PlatformModel;
using net.gensousakuya.dice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GensouSakuya.QQBot.Core.Handlers
{
    [Command("ask")]
    internal class AskHandler : IMessageCommandHandler
    {
        public async Task<bool> ExecuteAsync(MessageSource source, IEnumerable<string> command, List<BaseMessage> originMessage, SourceFullInfo sourceInfo)
        {
            var sourceMessageId = (originMessage?.FirstOrDefault() as SourceMessage)?.Id ?? default;
            var messages = new List<BaseMessage>();
            if (source.IsTraditionSource)
                messages.Add(new ReplyMessage(source.GroupIdNum.Value, source.QQNum.Value, sourceMessageId));
            
            await Task.Yield();
            if (command.Count() < 1)
            {
                messages.Add(new TextMessage("不提问怎么帮你选0 0？"));
                MessageManager.SendToSource(source, messages);
                return false;
            }

            string quest = null;
            if (command.Count() < 2)
            {
                if (command.ElementAt(0).Contains("|"))
                {
                    //当没提问时直接忽略处理问题，优化体验
                }
                else
                {
                    messages.Add(new TextMessage("快把你打算的选择告诉我"));
                    MessageManager.SendToSource(source, messages);
                    return false;
                }
            }
            else
            {
                quest = command.First();
                command = command.Skip(1);
            }

            //为了处理选项中有空格的情况
            var ansStr = string.Join(" ", command);
            var ans = ansStr.Split('|').ToList();
            var res = Ask(ans);
            var message = string.Join("\n", res.Select(p => $"{p.Quest}:{p.Percent}%")) + "\n"
                + $"{DataManager.Instance.BotName}觉得{string.Join("、", GetMax(res).Select(p => p.Quest))}比较好";
            if (!string.IsNullOrWhiteSpace(quest))
            {
                message = $"关于[{quest}]：\n" + message;
            }
            messages.Add(new TextMessage(message));
            MessageManager.SendToSource(source, messages);
            return true;
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
