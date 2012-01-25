using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DillPickle.Framework.Types
{
    public static class Time
    {
        public static Func<DateTime> Now = () => DateTime.UtcNow;

        public static DateTime Today()
        {
            return Now().Date;
        }

        public static void Reset()
        {
            Now = () => DateTime.UtcNow;
        }

        public static void SetTime(DateTime dateTime)
        {
            Now = () => dateTime;
        }
    }
}
