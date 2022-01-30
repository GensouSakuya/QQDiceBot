using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using GensouSakuya.QQBot.Core.Base;
using GensouSakuya.QQBot.Core.Commands;
using GensouSakuya.QQBot.Core.Commands.V2;
using GensouSakuya.QQBot.Core.Model;
using GensouSakuya.QQBot.Core.PlatformModel;
using GensouSakuya.QQBot.Core.QQManager;
using net.gensousakuya.dice;
using Group = GensouSakuya.QQBot.Core.Model.Group;

namespace GensouSakuya.QQBot.Core
{
    public class CommandCenter
    {
        private static readonly Logger _logger = Logger.GetLogger<CommandCenter>();
        private static readonly Random Random = new Random();
        private static readonly Dictionary<string, Type> Managers = new Dictionary<string, Type>();
        private static CommanderEngine _engine;

        public static void ReloadManagers()
        {
            _logger.Debug("ReloadingManager");
            //只能拿到直接继承的派生类，有需要的时候再改
            var managerTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(p => p.BaseType == typeof(BaseManager)).ToList();
            _logger.Debug($"found {managerTypes.Count} managers");
            var managerWithCommands = managerTypes.Select(p => new
            {
                Type = p,
                CommandList = p.GetCustomAttributes<CommandAttribute>().ToList()
            }).Where(p => p.CommandList.Any()).ToList();
            managerWithCommands.ForEach(p => { p.CommandList.ForEach(q => { Managers.Add(q.Command, p.Type); }); });
            _logger.Debug($"found {managerWithCommands.Count} valid v1 managers");

            var commanderTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(p => p.BaseType == typeof(BaseCommanderV2)).ToList();
            _logger.Debug($"found {commanderTypes.Count} commanders");
            var commanders = new List<BaseCommanderV2>();
            commanders.AddRange(commanderTypes.Where(p => !p.IsAbstract).Select(p => (BaseCommanderV2)Activator.CreateInstance(p)));
            _engine = new CommanderEngine(commanders);
        }

        public static async Task Execute(string command, List<BaseMessage> originMessage, MessageSourceType sourceType,long? qqNo = null, long? groupNo = null)
        {
            UserInfo qq = null;
            Group group = null; 
            if (qqNo.HasValue)
            {
                qq = UserManager.Get(qqNo.Value);
            }

            GroupMember member = null;
            if(qqNo.HasValue && groupNo.HasValue)
            {
                member = await GroupMemberManager.Get(qqNo.Value, groupNo.Value);
                if (member != null && (DataManager.Instance?.GroupIgnore?.ContainsKey((member.GroupNumber, member.QQ)) ?? false))
                {
                    return;
                }
            }

            if (!command.StartsWith(".") && !command.StartsWith("/"))
            {
                if (sourceType == MessageSourceType.Group)
                {
                    await ExecuteWithoutCommand(command, originMessage, sourceType, qq, group, member);
                }
                return;
            }

            var commandStr = command.Remove(0, 1);
            var commandList = Tools.TakeCommandParts(commandStr);

            var commandName = commandList.FirstOrDefault();
            if (commandName == null)
                return;

            var manager = GetManagerByCommand(commandName);
            if (manager == null)
            {
                if (sourceType == MessageSourceType.Group)
                {
                    await ExecuteWithoutCommand(command, originMessage, sourceType, qq, group, member);
                }
                return;
            }

            commandList.RemoveAt(0);
            var args = commandList;
            if (sourceType == MessageSourceType.Group)
            {
                if (BanManager.QQBan.TryGetValue(member.QQ, out _))
                {
                    MessageManager.SendTextMessage(sourceType, "滚", member.QQ, member.GroupNumber);
                    return;
                }
                else if (BanManager.GroupBan.TryGetValue((member.QQ, member.GroupNumber), out _))
                {
                    MessageManager.SendTextMessage(sourceType, "滚", member.QQ, member.GroupNumber); 
                    return;
                }
            }

            await manager.ExecuteAsync(args, originMessage, sourceType, qq, group, member);
        }

        private static async Task ExecuteWithoutCommand(string message, List<BaseMessage> originMessage, MessageSourceType sourceType, UserInfo qq, Group group, GroupMember member)
        {
            if (await _engine.ExecuteAsync(originMessage, sourceType, qq, group, member))
            {
                return;
            }

            var managerList = new List<Tuple<BaseManager, List<string>>>();
            var randomRes = Random.Next(1, 101);
            if (member != null && RepeatManager.GroupRepeatConfig.ContainsKey(member.GroupNumber))
            {
                var config = RepeatManager.GroupRepeatConfig[member.GroupNumber];
                if (randomRes <= config.Percent)
                {
                    managerList.Add(new Tuple<BaseManager, List<string>>(new RepeatManager(), new List<string>
                    {
                        "repeat", message
                    }));
                }
            }
            if (member != null && ShaDiaoTuManager.GroupShaDiaoTuConfig.ContainsKey(member.GroupNumber))
            {
                var config = ShaDiaoTuManager.GroupShaDiaoTuConfig[member.GroupNumber];
                if (randomRes <= config.Percent)
                {
                    managerList.Add(new Tuple<BaseManager, List<string>>(new ShaDiaoTuManager(), new List<string>
                    {
                        "shadiaotu"
                    }));
                }
            }
            if (member != null && BakiManager.GroupBakiConfig.ContainsKey(member.GroupNumber))
            {
                var config = BakiManager.GroupBakiConfig[member.GroupNumber];
                if (randomRes <= config.Percent)
                {
                    managerList.Add(new Tuple<BaseManager, List<string>>(new BakiManager(), new List<string>
                    {
                        "baki"
                    }));
                }
            }

            if (managerList.Any())
            {
                var choose = Random.Next(0, managerList.Count);
                var choosen = managerList[choose];
                Task.Run(async () =>
                {
                    await choosen.Item1.ExecuteAsync(choosen.Item2, originMessage, sourceType, qq, null, member);
                });
            }
        }

        public static BaseManager GetManagerByCommand(string command)
        {
            var key = command.ToLower();
            if (Managers.ContainsKey(key))
            {
                return (BaseManager) Activator.CreateInstance(Managers[key]);
            }
            return null;
        }
    }
}
