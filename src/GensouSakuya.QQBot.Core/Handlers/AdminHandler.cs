using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GensouSakuya.QQBot.Core.Base;
using GensouSakuya.QQBot.Core.Interfaces;
using GensouSakuya.QQBot.Core.Model;
using GensouSakuya.QQBot.Core.PlatformModel;

namespace GensouSakuya.QQBot.Core.Commands
{
    internal class AdminHandler : IMessageCommandHandler
    {
        public async Task ExecuteAsync(MessageSource source, IEnumerable<string> command, List<BaseMessage> originMessage, SourceFullInfo sourceInfo)
        {
            await Task.Yield();
            if (source.Type != MessageSourceType.Private && source.Type != MessageSourceType.Friend)
                return;
            if (sourceInfo.QQ == null || sourceInfo.QQ.QQ != DataManager.Instance.AdminQQ)
                return;
            var firstCommand = command.FirstOrDefault();
            if (firstCommand == null)
                return;
            switch (firstCommand.ToLower())
            {
                case "say":
                {
                    command = command.Skip(1);
                    if (command.Count() < 2)
                        return;
                    if (!long.TryParse(command.First(), out var groupNumber))
                    {
                        return;
                    }

                    command = command.Skip(1);
                    var message = string.Join(" ", command);
                    MessageManager.SendTextMessageToGroup(groupNumber, message);
                    return;
                }
                case "rename":
                    command = command.Skip(1);
                    if (command.Count() < 1)
                        return;
                    var name = command.ElementAt(0);
                    DataManager.Instance.BotName = name;
                    MessageManager.SendTextMessage(MessageSourceType.Friend, "改名成功", DataManager.Instance.AdminQQ);
                    return;
                case "save":
                    DataManager.Save();
                    MessageManager.SendTextMessage(MessageSourceType.Friend, "保存成功", DataManager.Instance.AdminQQ);
                    return;
            }
        }
    }
}
