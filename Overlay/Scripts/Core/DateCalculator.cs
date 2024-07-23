
namespace Overlay
{
    using Godot;
    using Godot.Collections;

    public sealed class DateLength
    {
        public int Year { get; set; } = 0;
        public int Month { get; set; } = 0;
        public int Day { get; set; } = 0;

        public int Hour { get; set; } = 0;
        public int Minute { get; set; } = 0;
        public int Second { get; set; } = 0;
    }

    public static class DateCalculator
    {
        public static DateLength CalculateTimeDifference(
            string timeStart,
            string timeEnd
        )
        {
            var startLength = RetrieveDateLengthFromTime(
                time: timeStart
            );
            var endLength = RetrieveDateLengthFromTime(
                time: timeEnd
            );

            var totalLength = new DateLength()
            {
                // calculate number of years
                Year = endLength.Year - startLength.Year
            };

            if (
                totalLength.Year > 0u &&
                (
                    startLength.Month > endLength.Month ||
                    (
                        startLength.Month == endLength.Month &&
                        startLength.Day >= endLength.Day &&
                        startLength.Hour >= endLength.Hour &&
                        startLength.Minute >= endLength.Minute &&
                        startLength.Second > endLength.Second
                    )
                )
            )
            {
                totalLength.Year--;
            }

            // calculate number of months
            totalLength.Month = 
                endLength.Month < startLength.Month ? 
                    c_numberOfMonths - startLength.Month + endLength.Month : 
                    endLength.Month - startLength.Month;

            // calculate number of days
            var monthStart = (Month)startLength.Month;
            var monthEnd = (Month)endLength.Month;

            var daysInMonthStart = IsLeapYear(
                year: startLength.Year
            ) && monthStart is Month.February ? 
                c_leapYearDays : 
                c_monthLengths[key: monthStart];

            var daysInMonthEnd = IsLeapYear(
                year: endLength.Year
            ) && monthStart is Month.February ? 
                c_leapYearDays : 
                c_monthLengths[key: monthEnd];

            var totalDaysInBothMonths = daysInMonthStart + daysInMonthEnd;
            var averageDaysInBothMonths = Mathf.CeilToInt(
                s: totalDaysInBothMonths / 2f
            );

            var daysRemainingInLengthStart = daysInMonthStart - startLength.Day;
            var daysPassedInMonthEnd = endLength.Day;
            var totalDaysFromStartToEnd = daysRemainingInLengthStart + daysPassedInMonthEnd;
            if (
                startLength.Hour > endLength.Hour ||
                (
                    startLength.Hour == endLength.Hour &&
                    startLength.Minute >= endLength.Minute &&
                    startLength.Second > endLength.Second
                )
            )
            {
                totalDaysFromStartToEnd--;
            }

            // less than a month of days have been consumed, remove a month.
            if (totalDaysFromStartToEnd < averageDaysInBothMonths)
            {
                totalLength.Month--;
                totalLength.Day = totalDaysFromStartToEnd;
            }
            else
            {
                totalLength.Day = totalDaysFromStartToEnd - averageDaysInBothMonths;
            }

            // calculate number of hours
            totalLength.Hour = c_numberOfHours - startLength.Hour + endLength.Hour;
            if (totalLength.Hour >= c_numberOfHours)
            {
                totalLength.Hour -= c_numberOfHours;
            }

            if (startLength.Minute > endLength.Minute)
            {
                totalLength.Hour--;
            }

            // calculate number of minutes
            totalLength.Minute = c_numberOfMinutes - startLength.Minute + endLength.Minute;
            if (totalLength.Minute >= c_numberOfMinutes)
            {
                totalLength.Minute -= c_numberOfMinutes;
            }

            if (totalLength.Second > endLength.Second)
            {
                totalLength.Minute--;
            }

            // calculate number of seconds
            totalLength.Second = c_numberOfSeconds - startLength.Second + endLength.Second;
            if (totalLength.Second >= c_numberOfSeconds)
            {
                totalLength.Second -= c_numberOfSeconds;
            }

            return totalLength;
        }

        private const int c_numberOfSeconds = 60;
        private const int c_numberOfMinutes = 60;
        private const int c_numberOfHours = 24;
        private const int c_numberOfMonths = 12;

        private const int c_utcIndexYear = 0;
        private const int c_utcIndexMonth = 5;
        private const int c_utcIndexDay = 8;
        private const int c_utcIndexHour = 11;
        private const int c_utcIndexMinute = 14;
        private const int c_utcIndexSecond = 17;

        private const int c_utcLengthYear = 4;
        private const int c_utcLengthMonth = 2;
        private const int c_utcLengthDay = 2;
        private const int c_utcLengthHour = 2;
        private const int c_utcLengthMinute = 2;
        private const int c_utcLengthSecond = 2;

        private const int c_leapYearDays = 29;

        private enum Month : uint
        {
            January = 1u,
            February,
            March,
            April,
            May,
            June,
            July,
            August,
            September,
            October,
            November,
            December,
        }

        private static readonly Dictionary<Month, int> c_monthLengths = new()
        {
            { Month.January,   31 },
            { Month.February,  28 },
            { Month.March,     31 },
            { Month.April,     30 },
            { Month.May,       31 },
            { Month.June,      30 },
            { Month.July,      31 },
            { Month.August,    31 },
            { Month.September, 30 },
            { Month.October,   31 },
            { Month.November,  30 },
            { Month.December,  31 },
        };

        private static DateLength RetrieveDateLengthFromTime(
            string time
        )
        {
            return new DateLength()
            {
                Year = time.Substr(
                    from: c_utcIndexYear,
                    len: c_utcLengthYear
                ).ToInt(),
                Month = time.Substr(
                    from: c_utcIndexMonth,
                    len: c_utcLengthMonth
                ).ToInt(),
                Day = time.Substr(
                   from: c_utcIndexDay,
                    len: c_utcLengthDay
                ).ToInt(),

                Hour = time.Substr(
                    from: c_utcIndexHour,
                    len: c_utcLengthHour
                ).ToInt(),
                Minute = time.Substr(
                    from: c_utcIndexMinute,
                    len: c_utcLengthMinute
                ).ToInt(),
                Second = time.Substr(
                    from: c_utcIndexSecond,
                    len: c_utcLengthSecond
                ).ToInt()
            };
        }

        private static bool IsLeapYear(
            int year
        )
        {
            return year % 4u is 0u;
        }
    }
}