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
    }
}
