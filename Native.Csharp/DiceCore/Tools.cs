using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace net.gensousakuya.dice
{
    public static class Tools
    {
        private static readonly List<string> separators = new List<string>
        {
            @"\s"
        };

        private static Regex splitRegex = new Regex($@"[{string.Join("", separators)}]");

        public static List<string> TakeCommandParts(string fullCommand)
        {
            return fullCommand.Split(separators.ToArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
        }
    }
}
