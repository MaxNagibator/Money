namespace Money.Web.Common;

public class DarkModeSettings
{
    public TimeSpan? DarkStart { get; set; } = new(18, 0, 0);
    public TimeSpan? DarkEnd { get; set; } = new(6, 0, 0);
    public TimeSpan CheckInterval { get; set; } = new(0, 5, 0);

    public bool IsLocationChangedEnabled { get; set; } = true;
    public bool IsTimerEnabled { get; set; } = true;
}
