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
using net.gensousakuya.dice;

namespace GensouSakuya.QQBot.Core
{
    internal class CommandCenter
    {
        private static readonly Random Random = new Random();
        private static readonly Dictionary<string, Type> Managers = new Dictionary<string, Type>();
        private static CommanderEngine _engine;

        public static void ReloadManagers()
        {
            //只能拿到直接继承的派生类，有需要的时候再改
            var managerTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(p => p.BaseType == typeof(BaseManager)).ToList();
            var managerWithCommands = managerTypes.Select(p => new
            {
                Type = p,
                CommandList = p.GetCustomAttributes<CommandAttribute>().ToList()
            }).Where(p => p.CommandList.Any()).ToList();
            managerWithCommands.ForEach(p => { p.CommandList.ForEach(q => { Managers.Add(q.Command, p.Type); }); });

            var commanderTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(p => p.BaseType == typeof(BaseCommanderV2)).ToList();
            var commanders = new List<BaseCommanderV2>();
            commanders.AddRange(commanderTypes.Where(p => !p.IsAbstract).Select(p => (BaseCommanderV2)Activator.CreateInstance(p)));
            _engine = new CommanderEngine(commanders);
        }

        public static async Task Execute(MessageSource source, string rawMessage, string command, IEnumerable<string> commandArgs, List<BaseMessage> originMessage, SourceFullInfo sourceInfo)
        {
            var manager = GetManagerByCommand(command);
            if (manager == null)
            {
                if (source.Type == MessageSourceType.Group || source.Type == MessageSourceType.Guild)
                {
                    await ExecuteWithoutCommand(source, rawMessage, originMessage, sourceInfo);
                }
                return;
            }

            await manager.ExecuteAsync(source, commandArgs?.ToList(), originMessage, sourceInfo.QQ, sourceInfo.Group, sourceInfo.GroupMember, sourceInfo.GuildUser, sourceInfo.GuildMember);
        }

        internal static async Task ExecuteWithoutCommand(MessageSource source, string message, List<BaseMessage> originMessage, SourceFullInfo sourceInfo)
        {
            if (source.Type != MessageSourceType.Group && source.Type != MessageSourceType.Guild)
            {
                return;
            }
            if (await _engine.ExecuteAsync(source, originMessage, sourceInfo.QQ, sourceInfo.Group, sourceInfo.GroupMember, sourceInfo.GuildUser, sourceInfo.GuildMember))
            {
                return;
            }

            var managerList = new List<Tuple<BaseManager, List<string>>>();
            var randomRes = Random.Next(1, 101);
            var member = sourceInfo.GroupMember;
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
                _ = choosen.Item1.ExecuteAsync(source, choosen.Item2, originMessage, sourceInfo.QQ, null, member, null, null);
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
