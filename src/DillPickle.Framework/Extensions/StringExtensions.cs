using System.Collections;
using System.Linq;

namespace System
{
    public static class StringExtensions
    {
        public static string JoinToString(this IEnumerable enumerable, string separator)
        {
            var strings = enumerable.Cast<object>().Select(e => e == null ? "(null)" : e.ToString());

            return string.Join(separator, strings.ToArray());
        }

        public static bool IsSet(this string str)
        {
            return !string.IsNullOrEmpty(str) && str.Trim() != "";
        }
    }
}