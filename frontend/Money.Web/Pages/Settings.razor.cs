using Microsoft.AspNetCore.Components;

namespace Money.Web.Pages;

public partial class Settings
{
    private bool _isInit;

    [CascadingParameter]
    public required AppSettings AppSettings { get; set; }

    [CascadingParameter]
    public required DarkModeToggle ModeToggle { get; set; }

    [Inject]
    private ISnackbar SnackbarService { get; set; } = null!;

    private TimeSpan? ScheduleCheckInterval { get; set; }
    private TimeSpan? DarkModeStart { get; set; }
    private TimeSpan? DarkModeEnd { get; set; }

    private bool DisabledSchedule => !AppSettings.IsSchedule || AppSettings.IsManualMode;

    protected override async Task OnInitializedAsync()
    {
        var modeSettings = await ModeToggle.GetSettingsAsync();
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
