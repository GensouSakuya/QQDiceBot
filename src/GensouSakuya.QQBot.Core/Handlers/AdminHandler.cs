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
        private readonly DataManager _dataManager;
        public AdminHandler(DataManager data) 
        { 
            _dataManager = data;
        }

        public async Task<bool> ExecuteAsync(MessageSource source, IEnumerable<string> command, List<BaseMessage> originMessage, SourceFullInfo sourceInfo)
        {
            await Task.Yield();
            if (source.Type != MessageSourceType.Private && source.Type != MessageSourceType.Friend)
                return false;
            if (sourceInfo.QQ == null || sourceInfo.QQ.QQ != _dataManager.Config.AdminQQ)
                return false;
            var firstCommand = command.FirstOrDefault();
            if (firstCommand == null)
                return false;
            switch (firstCommand.ToLower())
            {
                case "say":
                {
                    command = command.Skip(1);
                    if (command.Count() < 2)
                        return false;
                    if (!long.TryParse(command.First(), out var groupNumber))
                    {
                        return false;
                    }

                    command = command.Skip(1);
                    var message = string.Join(" ", command);
                    MessageManager.SendTextMessageToGroup(groupNumber, message);
                    return true;
                }
                case "rename":
                    command = command.Skip(1);
                    if (command.Count() < 1)
                        return false;
                    var name = command.ElementAt(0);
                    _dataManager.Config.BotName = name;
                    MessageManager.SendTextMessage(MessageSourceType.Friend, "改名成功", _dataManager.Config.AdminQQ);
                    return true;
                case "save":
                    await _dataManager.Save();
                    MessageManager.SendTextMessage(MessageSourceType.Friend, "保存成功", _dataManager.Config.AdminQQ);
                    return true;
            }
            return true;
        }
    }
}
