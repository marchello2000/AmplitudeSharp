using System;

namespace AmplitudeSharp.Utils
{
    static class DateTimeExtensions
    {
        /// <summary>
        /// Converts a DateTime to unix time.
        /// </summary>
        /// <param name="dateTime">A DateTime to convert to epoch time (note, will be converted to UTC).</param>
        /// <returns>Unix time of the given date</returns>
        public static long ToUnixEpoch(this DateTime dateTime) => (long)(dateTime.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;

        /// <summary>
        /// Converts a unix time to a DateTime.
        /// </summary>
        /// <param name="epoch">Number of seconds since Jan 1, 1970.</param>
        /// <returns>DateTime representing epoch</returns>
        public static DateTime FromEpoch(long epoch) => new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(epoch);

    }
}
