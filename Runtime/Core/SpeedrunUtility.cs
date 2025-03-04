/**
 * Sideways Experiments (c) 2025
 * https://sideways-experiments.com
 * Contact: dev@side-xp.com
 */

using System;

namespace SideXP.Speedrun
{

    /// <summary>
    /// Miscellaneous functions for working with Speedrun features.
    /// </summary>
    public static class SpeedrunUtility
    {

        /// <summary>
        /// Gets the total milliseconds elapsed from 0001-01-01 to the current date and time.
        /// </summary>
        public static long GetMillisecondsToNow()
        {
            return GetMillisecondsToDate(DateTime.Now);
        }

        /// <summary>
        /// Gets the total milliseconds elapsed from 0001-01-01 to the given date and time.
        /// </summary>
        /// <param name="dateTime">The date and time to convert as milliseconds.</param>
        public static long GetMillisecondsToDate(this DateTime dateTime)
        {
            return dateTime.Ticks / TimeSpan.TicksPerMillisecond;
        }

        /// <summary>
        /// Gets the date and time from a given amount of milliseconds elapsed from 0001-01-01.
        /// </summary>
        /// <param name="milliseconds">The number of milliseconds to convert.</param>
        public static DateTime ToDateTime(this long milliseconds)
        {
            return new DateTime(milliseconds * TimeSpan.TicksPerMillisecond);
        }

    }

}