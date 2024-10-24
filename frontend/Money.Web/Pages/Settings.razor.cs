using Microsoft.AspNetCore.Components;
using Money.Web.Components;

namespace Money.Web.Pages;

public partial class Settings
{
    private bool _isInit;

    [CascadingParameter]
    public AppSettings AppSettings { get; set; } = default!;

    [CascadingParameter]
    public DarkModeToggle ModeToggle { get; set; } = default!;

    [Inject]
    private ISnackbar SnackbarService { get; set; } = default!;

    private TimeSpan? ScheduleCheckInterval { get; set; }
    private TimeSpan? DarkModeStart { get; set; }
    private TimeSpan? DarkModeEnd { get; set; }

    private bool DisabledSchedule => !AppSettings.IsSchedule || AppSettings.IsManualMode;

    protected override async Task OnInitializedAsync()
    {
        DarkModeSettings modeSettings = await ModeToggle.GetSettingsAsync();
        DarkModeStart = modeSettings.DarkStart;
        DarkModeEnd = modeSettings.DarkEnd;
        ScheduleCheckInterval = modeSettings.CheckInterval;
        _isInit = true;
    }

    private async Task UpdateSchedule()
    {
        await ModeToggle.SetScheduledMode(DarkModeStart, DarkModeEnd);
        await ModeToggle.SetInterval(ScheduleCheckInterval);
        SnackbarService.Add("Расписание успешно обновлено!", Severity.Success);
    }
}
