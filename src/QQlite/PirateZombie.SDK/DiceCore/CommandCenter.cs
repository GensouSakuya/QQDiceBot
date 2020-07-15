using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static net.gensousakuya.dice.Tools;

namespace net.gensousakuya.dice
{
    public static class CommandCenter
    {
        private static readonly Random Random = new Random();
        private static readonly Dictionary<string, Type> Managers = new Dictionary<string, Type>();

        public static void ReloadManagers()
        {
            var managerTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(p => p.IsAssignableFrom(typeof(BaseManager))).ToList();
            var managerWithCommands = managerTypes.Select(p => new
            {
                Type = p,
                CommandList = p.GetCustomAttributes<CommandAttribute>().ToList()
            }).Where(p => p.CommandList.Any()).ToList();
            managerWithCommands.ForEach(p => { p.CommandList.ForEach(q => { Managers.Add(q.Command, p.Type); }); });
        }

        public static void Execute(string command, EventSourceType sourceType,long? qqNo = null, long? groupNo = null)
        {
            UserInfo qq = null;
            if(qqNo.HasValue)
            {
                qq = UserManager.Get(qqNo.Value);
            }
            Group group = null;

            GroupMember member = null;
            if(qqNo.HasValue && groupNo.HasValue)
            {
                member = GroupMemberManager.Get(qqNo.Value, groupNo.Value);
            }

            if (!command.StartsWith(".") && !command.StartsWith("/"))
            {
                ExecuteWithoutCommand(command, sourceType, qq, member);
                return;
            }

            var commandStr = command.Remove(0, 1);
            var commandList = TakeCommandParts(commandStr);

            var commandName = commandList.FirstOrDefault();
            if (commandName == null)
                return;

            var manager = GetManagerByCommand(commandName);
            if (manager == null)
            {
                ExecuteWithoutCommand(command, sourceType, qq, member);
                return;
            }

            commandList.RemoveAt(0);
            var args = commandList;
            Task.Run(async () => { await manager.ExecuteAsync(args, sourceType, qq, group, member); });
        }

        private static void ExecuteWithoutCommand(string message, EventSourceType sourceType, UserInfo qq, GroupMember member)
        {
            BaseManager manager = null;
            var commands = new List<string>();
            if (manager == null && member != null && DataManager.Instance.GroupRepeatConfig.ContainsKey(member.GroupNumber))
            {
                var config = DataManager.Instance.GroupRepeatConfig[member.GroupNumber];
                var rdm = Random.Next(1, 101);
                if (rdm <= config.Percent)
                {
                    manager = new RepeatManager();
                    commands.Add("repeat");
                    commands.Add(message);
                }
            }
            if (manager == null && member != null && DataManager.Instance.GroupShaDiaoTuConfig.ContainsKey(member.GroupNumber))
            {
                var config = DataManager.Instance.GroupShaDiaoTuConfig[member.GroupNumber];
                var rdm = Random.Next(1, 101);
                if (rdm <= config.Percent)
                {
                    manager = new ShaDiaoTuManager();
                    commands.Add("shadiaotu");
                    commands.Add(message);
                }
            }
            if (manager == null && member != null && DataManager.Instance.GroupBakiConfig.ContainsKey(member.GroupNumber))
            {
                var config = DataManager.Instance.GroupBakiConfig[member.GroupNumber];
                var rdm = Random.Next(1, 101);
                if (rdm <= config.Percent)
                {
                    manager = new BakiManager();
                    commands.Add("baki");
                    commands.Add(message);
                }
            }

            if (manager!=null)
            {
                Task.Run(async () =>
                {
                    await manager.ExecuteAsync(commands, sourceType, qq, null, member);
                });
            }
        }

        //后面可以改成把对应命令绑定在Attribute里
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
