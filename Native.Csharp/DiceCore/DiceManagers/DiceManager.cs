using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace net.gensousakuya.dice
{
    public class DiceManager : BaseManager
    {
        private static Random _rand = new Random();

        public override void Execute(List<string> command, EventSourceType sourceType, UserInfo qq, Group groupNo, GroupMember member)
        {
            throw new NotImplementedException();
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
