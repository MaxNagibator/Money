namespace Money.Web.Models;

public record DateInterval(string DisplayName, Func<DateTime, DateTime> Start, Func<DateTime, DateTime> End);
