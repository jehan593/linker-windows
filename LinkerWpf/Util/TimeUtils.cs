using System;

namespace Linker.Util
{
    public static class TimeUtils
    {
        public static string FormatSavedTime(long millis) =>
            DateTimeOffset.FromUnixTimeMilliseconds(millis).ToLocalTime().ToString("t");

        public static DateTime DayKey(long millis) =>
            DateTimeOffset.FromUnixTimeMilliseconds(millis).ToLocalTime().Date;

        public static string DayLabel(DateTime day)
        {
            var today = DateTime.Today;
            if (day == today) return "Today";
            if (day == today.AddDays(-1)) return "Yesterday";
            return day.ToString("MMM d, yyyy");
        }
    }
}
