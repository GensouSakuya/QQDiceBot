using System;

namespace GensouSakuya.QQBot.Core.Base
{
    internal class DefaultAgentAttribute:Attribute
    {
        public string Id { get; set; }
        public DefaultAgentAttribute(string id)
        {
            Id = id;
        }
    }
}
