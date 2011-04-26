using System;
using System.Collections.Generic;

namespace DillPickle.Framework.Extensions
{
    public static class EnumerableExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> handleItem)
        {
            foreach(var item in enumerable)
            {
                handleItem(item);
            }
        }
    }
}