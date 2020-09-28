using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GensouSakuya.QQBot.Core.Base;
using GensouSakuya.QQBot.Core.Model;
using GensouSakuya.QQBot.Core.PlatformModel;
using net.gensousakuya.dice;

namespace GensouSakuya.QQBot.Core.Commands
{
    [Command("admin")]
    public class AdminManager : BaseManager
    {
        public override async Task ExecuteAsync(List<string> command, MessageSourceType sourceType, UserInfo qq, Group group, GroupMember member)
        {
            await Task.Yield();
            if (sourceType != MessageSourceType.Private && sourceType != MessageSourceType.Friend)
                return;
            if (qq == null || qq.QQ != DataManager.Instance.AdminQQ)
                return;
            var firstCommand = command.FirstOrDefault();
            if (firstCommand == null)
                return;
            switch (firstCommand.ToLower())
            {
                case "say":
                    {
                        command.RemoveAt(0);
                        if (command.Count < 2)
                            return;
                        var groupNumber = 0L;
                        if (!long.TryParse(command.First(), out groupNumber))
                        {
                            return;
                        }

                        command.RemoveAt(0);
                        var message = string.Join(" ", command);
                        MessageManager.SendTextMessage(MessageSourceType.Group, message, qq?.QQ, groupNumber);
                        return;
                    }
                case "rename":
                    command.RemoveAt(0);
                    if (command.Count < 1)
                        return;
                    var name = command[0];
                    DataManager.Instance.BotName = name;
                    MessageManager.SendTextMessage(MessageSourceType.Friend, "改名成功", qq?.QQ);
                    return;
                case "save":
                    DataManager.Save();
                    MessageManager.SendTextMessage(MessageSourceType.Friend,"保存成功", qq?.QQ);
                    return;
            }
        }
    }
}
