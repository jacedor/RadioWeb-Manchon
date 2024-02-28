using System;

namespace ADPM.Common.Extensions
{

    /// <summary>
    /// Extension methods for System.DateTime.
    /// </summary>
    public static class DateExtensions
    {

        /// <summary>
        /// Extension method to return the first day of the month.
        /// </summary>
        public static DateTime FirstOfMonth(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1);
        }

        public static DateTime LastOfYear(this DateTime date)
        {
            return new DateTime(date.Year, 12, 31);
        }

        /// <summary>
        /// Extension method to return the last day of the month.
        /// </summary>
        public static DateTime LastOfMonth(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1).AddMonths(1).AddDays(-1);
        }

        /// <summary>
        /// Extension method to return the first working day of the week (Monday).
        /// </summary>
        public static DateTime FirstOfWorkingWeek(this DateTime date)
        {
            return date.AddDays(DayOfWeek.Monday - date.DayOfWeek);
        }

        /// <summary>
        /// Extension method to return the last working day of the week (Friday).
        /// </summary>
        public static DateTime LastOfWorkingWeek(this DateTime date)
        {
            return date.AddDays(DayOfWeek.Friday - date.DayOfWeek);
        }

        /// <summary>
        /// Extension method to return the first  day of the week (Sunday).
        /// </summary>
        public static DateTime FirstOfWeek(this DateTime date)
        {
            return date.AddDays(DayOfWeek.Sunday - date.DayOfWeek);
        }

        /// <summary>
        /// Extension method to return the last  day of the week (Saturday).
        /// </summary>
        public static DateTime LastOfWeek(this DateTime date)
        {
            return date.AddDays(DayOfWeek.Saturday - date.DayOfWeek);
        }

        public static bool IsWeekEnd(this DateTime date)
        {
            return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;         
        }

    }

}
