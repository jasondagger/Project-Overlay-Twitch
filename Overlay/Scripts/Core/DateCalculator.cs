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
        DateLength startLength = RetrieveDateLengthFromTime(
            timeStart
        );
        DateLength endLength = RetrieveDateLengthFromTime(
            timeEnd
        );

        DateLength totalLength = new();

        // calculate number of years
        totalLength.Year = endLength.Year - startLength.Year;
        if (
            totalLength.Year > 0u && 
            (
                startLength.Month > endLength.Month ||
                (                
                    startLength.Month  == endLength.Month  &&
                    startLength.Day    >= endLength.Day    &&
                    startLength.Hour   >= endLength.Hour   &&
                    startLength.Minute >= endLength.Minute &&
                    startLength.Second >  endLength.Second 
                )
            )
        )
        {
            totalLength.Year--;
        }

        // calculate number of months
        totalLength.Month = endLength.Month < startLength.Month ? c_numberOfMonths - startLength.Month + endLength.Month : endLength.Month - startLength.Month;

        // calculate number of days
        Month monthStart = (Month)startLength.Month;
        Month monthEnd = (Month)endLength.Month;

        int daysInMonthStart = IsLeapYear(
            startLength.Year
        ) && monthStart == Month.February ? c_leapYearDays : c_monthLengths[monthStart];
        int daysInMonthEnd = IsLeapYear(
            endLength.Year
        ) && monthStart == Month.February ? c_leapYearDays : c_monthLengths[monthEnd];

        int totalDaysInBothMonths = daysInMonthStart + daysInMonthEnd;
        int averageDaysInBothMonths = Mathf.CeilToInt(
            totalDaysInBothMonths / 2f
        );

        int daysRemainingInLengthStart = daysInMonthStart - startLength.Day;
        int daysPassedInMonthEnd = endLength.Day;
        int totalDaysFromStartToEnd = daysRemainingInLengthStart + daysPassedInMonthEnd;
        if (
            startLength.Hour > endLength.Hour ||
            (
                startLength.Hour   == endLength.Hour   &&
                startLength.Minute >= endLength.Minute &&
                startLength.Second >  endLength.Second
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

        if (startLength.Minute  > endLength.Minute)
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
        DateLength dateLength = new();

        dateLength.Year = time.Substr(
            c_utcIndexYear,
            c_utcLengthYear
        ).ToInt();
        dateLength.Month = time.Substr(
            c_utcIndexMonth,
            c_utcLengthMonth
        ).ToInt();
        dateLength.Day = time.Substr(
            c_utcIndexDay,
            c_utcLengthDay
        ).ToInt();

        dateLength.Hour = time.Substr(
            c_utcIndexHour,
            c_utcLengthHour
        ).ToInt();
        dateLength.Minute = time.Substr(
            c_utcIndexMinute,
            c_utcLengthMinute
        ).ToInt();
        dateLength.Second = time.Substr(
            c_utcIndexSecond,
            c_utcLengthSecond
        ).ToInt();

        return dateLength;
    }

    private static bool IsLeapYear(
        int year    
    )
    {
        return year % 4u == 0u;
    }
}
