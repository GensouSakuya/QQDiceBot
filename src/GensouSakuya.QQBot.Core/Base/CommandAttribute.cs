using System;

namespace GensouSakuya.QQBot.Core.Base
{
    public class CommandAttribute : Attribute
    {
        public string Command { get; }
        public CommandAttribute(string command)
        {
            Command = command;
        }
    }
}
