using System.Collections;
using System.Linq;

namespace DillPickle.Framework.Extensions
{
    public static class ObjectExtensions
    {
        public static bool In(this object obj, IEnumerable enumerable)
        {
            return enumerable.Cast<object>().Any(obj.Equals);
        }
    }
}