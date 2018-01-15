using System;

namespace Neo.UI.Core.Helpers
{
    internal static class TimeHelper
    {
        public static DateTime UnixTimestampToDateTime(uint timeStamp)
        {
            // Unix timestamp is the numbers of seconds past the epoch
            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

            dateTime = dateTime.AddSeconds(timeStamp).ToLocalTime();

            return dateTime;
        }
    }
}
