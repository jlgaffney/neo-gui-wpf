using System;

namespace Neo.Gui.Base.Helpers
{
    public static class TimeHelper
    {
        public static DateTime UnixTimestampToDateTime(uint timeStamp)
        {
            // Unix timestamp is seconds past epoch
            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

            dateTime = dateTime.AddSeconds(timeStamp).ToLocalTime();

            return dateTime;
        }
    }
}
