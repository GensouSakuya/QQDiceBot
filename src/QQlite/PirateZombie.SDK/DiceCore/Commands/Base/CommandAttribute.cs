using System;

namespace net.gensousakuya.dice
{
    public class CommandAttribute:Attribute
    {
        public string Command { get; }
        public CommandAttribute(string command)
        {
            Command = command;
        }
    }
}
