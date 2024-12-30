using GensouSakuya.QQBot.Core.Model;
using System;

namespace GensouSakuya.QQBot.Core
{
    public static class Extensions
    {
        public static long ToLong(this string str)
        {
            return long.TryParse(str, out var num) ? num : default;
        }

        public static bool HasValue(this string str)
        {
            return !string.IsNullOrWhiteSpace(str);
        }

        public static Delegate Attach(this Delegate baseAction, Delegate attached)
        {
            if (baseAction == null || attached == null)
                return baseAction;
            baseAction = attached;
            return baseAction;
        }

        public static bool IsGroupAdmin(this GroupMember gm)
        {
            return gm?.PermitType == PlatformModel.PermitType.Manage || gm?.PermitType == PlatformModel.PermitType.Holder;
        }
    }
}
