using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace FluffyBunny4.DotNetCore.Extensions
{
    public static class DateTimeExtensions
    {
        [DebuggerStepThrough]
        public static bool HasExceeded(this DateTime creationTime, int seconds, DateTime now)
        {
            return now > creationTime.AddSeconds(seconds);
        }

        [DebuggerStepThrough]
        public static int GetLifetimeInSeconds(this DateTime creationTime, DateTime now)
        {
            return (int)(now - creationTime).TotalSeconds;
        }
    }
}
