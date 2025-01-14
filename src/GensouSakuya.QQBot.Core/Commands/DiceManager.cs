﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GensouSakuya.QQBot.Core.Base;
using GensouSakuya.QQBot.Core.Model;
using GensouSakuya.QQBot.Core.PlatformModel;

namespace net.gensousakuya.dice
{
    [Command("r")]
    public class DiceManager : BaseManager
    {
        private static Random _rand = new Random();

        public override async Task ExecuteAsync(MessageSource source, List<string> command, List<BaseMessage> originMessage, UserInfo qq, Group groupNo, GroupMember member, GuildUserInfo guildUser, GuildMember guildmember)
        {
            await Task.Yield();
            //throw new NotImplementedException();
        }

        public static List<int> RollMultiDice(int diceCount = 1, int surfaceCount = 100)
        {
            var result = new List<int>();
            for (int i = 1; i <= diceCount; i++)
            {
                result.Add(RollDice(surfaceCount));
            }

            return result;
        }

        public static int RollDice(int surfaceCount = 100)
        {
            return _rand.Next(1, surfaceCount + 1);
        }
    }
}
