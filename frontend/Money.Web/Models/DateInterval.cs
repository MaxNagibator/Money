namespace Money.Web.Models;

public record DateInterval(
    string DisplayName,
    string ChangeName,
    Func<DateTime, DateTime> Start,
    Func<DateTime, DateTime> End,
    Func<DateTime, int, DateTime> Change)
{
    public DateRange Increment(DateRange range)
    {
        return ChangeRange(range, 1);
    }

    public DateRange Decrement(DateRange range)
    {
        return ChangeRange(range, -1);
    }

    private DateRange ChangeRange(DateRange range, int changeAmount)
    {
        if (range.Start == null)
        {
            return range;
        }

        var start = Change.Invoke(range.Start.Value, changeAmount);
        var end = End.Invoke(start);
        return new(start, end);
    }
}
