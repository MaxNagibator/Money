namespace Money.Web.Common;

public static class DateTimeExtensions
{
    public static DateTime StartOfWeek(this DateTime date)
    {
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-1 * diff).Date;
    }

    public static DateTime EndOfWeek(this DateTime date)
    {
        return date.StartOfWeek().AddDays(6).Date;
    }

    public static DateTime StartOfMonth(this DateTime date)
    {
        return new(date.Year, date.Month, 1);
    }

    public static DateTime EndOfMonth(this DateTime date)
    {
        return new(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));
    }

    public static DateTime StartOfYear(this DateTime date)
    {
        return new(date.Year, 1, 1);
    }

    public static DateTime EndOfYear(this DateTime date)
    {
        return new(date.Year, 12, 31);
    }
}
