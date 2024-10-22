using Microsoft.AspNetCore.Components;
using Money.Web.Components;

namespace Money.Web.Pages;

public partial class Settings
{
    [CascadingParameter]
    public AppSettings AppSettings { get; set; } = default!;

    [CascadingParameter]
    public DarkModeToggle ModeToggle { get; set; } = default!;

    public TimeSpan? ScheduleCheckInterval { get; set; }

    [Inject]
    private ISnackbar SnackbarService { get; set; } = default!;

    private TimeSpan? DarkModeStart { get; set; }
    private TimeSpan? DarkModeEnd { get; set; }

    protected override void OnInitialized()
    {
        DarkModeStart = ModeToggle.Settings.DarkStart;
        DarkModeEnd = ModeToggle.Settings.DarkEnd;
        ScheduleCheckInterval = ModeToggle.Settings.CheckInterval;
    }

    private async Task UpdateSchedule()
    {
        await ModeToggle.SetScheduledMode(DarkModeStart, DarkModeEnd);
        await ModeToggle.SetInterval(ScheduleCheckInterval);
        SnackbarService.Add("Расписание успешно обновлено!", Severity.Success);
    }
}
