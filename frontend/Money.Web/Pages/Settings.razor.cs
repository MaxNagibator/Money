using Microsoft.AspNetCore.Components;
using Money.Web.Components;

namespace Money.Web.Pages;

public partial class Settings
{
    [CascadingParameter]
    public AppSettings AppSettings { get; set; } = default!;

    [CascadingParameter]
    public DarkModeToggle DarkModeToggle { get; set; } = default!;

    public TimeSpan? ScheduleCheckInterval { get; set; }

    private TimeSpan? DarkModeStart { get; set; }
    private TimeSpan? DarkModeEnd { get; set; }

    protected override void OnInitialized()
    {
        DarkModeStart = DarkModeToggle.DarkStart;
        DarkModeEnd = DarkModeToggle.DarkEnd;
        ScheduleCheckInterval = DarkModeToggle.CheckInterval;
    }

    private async Task UpdateSchedule()
    {
        await DarkModeToggle.SetScheduledMode(DarkModeStart, DarkModeEnd);
        await DarkModeToggle.SetInterval(ScheduleCheckInterval);
    }
}
