using System;
using System.Collections.Generic;
using System.Linq;
using GensouSakuya.QQBot.Core.Model;
using Newtonsoft.Json;

namespace GensouSakuya.QQBot.Core.Base
{
    public static class Tools
    {
        private static readonly List<string> separators = new List<string>
        {
            " "
        };

        public static List<string> TakeCommandParts(string fullCommand)
        {
            return fullCommand.Split(separators.ToArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
        }


        public static string SerializeObject(object obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.Indented);
        }
        
        public static T DeserializeObject<T>(string xml)
        {
            return JsonConvert.DeserializeObject<T>(xml);
        }

        public static bool IsRobotAdmin(long qq)
        {
            return DataManager.Instance?.AdminQQ == qq;
        }

        public static bool IsGroupAdmin(this GroupMember gm)
        {
            return gm?.PermitType == PlatformModel.PermitType.Manage || gm?.PermitType == PlatformModel.PermitType.Holder;
        }
    }
}
