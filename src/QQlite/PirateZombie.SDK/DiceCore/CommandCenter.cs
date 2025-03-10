﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PirateZombie.SDK;
using static net.gensousakuya.dice.Tools;

namespace net.gensousakuya.dice
{
    public static class CommandCenter
    {
        private static readonly Random Random = new Random();
        private static readonly Dictionary<string, Type> Managers = new Dictionary<string, Type>();

        public static void ReloadManagers()
        {
            QLAPI.Api_SendLog("debug", "ReloadingManager", 0, QLMain.ac);
            //只能拿到直接继承的派生类，有需要的时候再改
            var managerTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(p => p.BaseType == typeof(BaseManager)).ToList();
            QLAPI.Api_SendLog("debug", $"found {managerTypes.Count} managers", 0, QLMain.ac);
            var managerWithCommands = managerTypes.Select(p => new
            {
                Type = p,
                CommandList = p.GetCustomAttributes<CommandAttribute>().ToList()
            }).Where(p => p.CommandList.Any()).ToList();
            managerWithCommands.ForEach(p => { p.CommandList.ForEach(q => { Managers.Add(q.Command, p.Type); }); });
            QLAPI.Api_SendLog("debug", $"found {managerWithCommands.Count}", 0, QLMain.ac);
        }

        public static void Execute(string command, EventSourceType sourceType,long? qqNo = null, long? groupNo = null)
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
                member = GroupMemberManager.Get(qqNo.Value, groupNo.Value);
            }

            if (!command.StartsWith(".") && !command.StartsWith("/"))
            {
                if (sourceType == EventSourceType.Group)
                {
                    ExecuteWithoutCommand(command, sourceType, qq, member);
                }
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
                if (sourceType == EventSourceType.Group)
                {
                    ExecuteWithoutCommand(command, sourceType, qq, member);
                }
                return;
            }

            commandList.RemoveAt(0);
            var args = commandList;
            Task.Run(async () => { await manager.ExecuteAsync(args, sourceType, qq, group, member); });
        }

        private static void ExecuteWithoutCommand(string message, EventSourceType sourceType, UserInfo qq, GroupMember member)
        {
            var managerList = new List<Tuple<BaseManager, List<string>>>();
            var randomRes = Random.Next(1, 101);
            if (member != null && DataManager.Instance.GroupRepeatConfig.ContainsKey(member.GroupNumber))
            {
                var config = DataManager.Instance.GroupRepeatConfig[member.GroupNumber];
                if (randomRes <= config.Percent)
                {
                    managerList.Add(new Tuple<BaseManager, List<string>>(new RepeatManager(), new List<string>
                    {
                        "repeat", message
                    }));
                }
            }
            if (member != null && DataManager.Instance.GroupShaDiaoTuConfig.ContainsKey(member.GroupNumber))
            {
                var config = DataManager.Instance.GroupShaDiaoTuConfig[member.GroupNumber];
                if (randomRes <= config.Percent)
                {
                    managerList.Add(new Tuple<BaseManager, List<string>>(new ShaDiaoTuManager(), new List<string>
                    {
                        "shadiaotu"
                    }));
                }
            }
            if (member != null && DataManager.Instance.GroupBakiConfig.ContainsKey(member.GroupNumber))
            {
                var config = DataManager.Instance.GroupBakiConfig[member.GroupNumber];
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
                    await choosen.Item1.ExecuteAsync(choosen.Item2, sourceType, qq, null, member);
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
