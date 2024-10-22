using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;

namespace Money.Web.Components;

public partial class DarkModeToggle : IDisposable
{
    private PeriodicTimer? _timer;
    private Task? _scheduledTask;

    [Parameter]
    public AppSettings Settings { get; set; } = default!;

    public TimeSpan? DarkStart { get; set; } = new(18, 0, 0);
    public TimeSpan? DarkEnd { get; set; } = new(6, 0, 0);
    public TimeSpan CheckInterval { get; set; } = new(0, 5, 0);

    [Inject]
    private ILocalStorageService StorageService { get; set; } = default!;

    public async Task SetScheduledMode(TimeSpan? darkModeStartTime, TimeSpan? darkModeEndTime)
    {
        DarkStart = darkModeStartTime;
        DarkEnd = darkModeEndTime;

        await StorageService.SetItemAsync(nameof(DarkStart), darkModeStartTime);
        await StorageService.SetItemAsync(nameof(DarkEnd), darkModeEndTime);

        CheckScheduledMode();
    }

    // TODO Убрать дублирование
    public void ToggleDarkMode()
    {
        if (Settings is { IsManualMode: false })
        {
            ToggleManualMode();
        }

        Settings.IsDarkMode = !Settings.IsDarkMode;
    }

    public void ToggleManualMode()
    {
        Settings.IsManualMode = !Settings.IsManualMode;

        UpdateState();
    }

    public void ToggleScheduleMode()
    {
        Settings.IsSchedule = !Settings.IsSchedule;

        UpdateState();
    }

    public async Task SetInterval(TimeSpan? interval)
    {
        if (interval == null)
        {
            return;
        }

        CheckInterval = interval.Value;
        await StorageService.SetItemAsync(nameof(CheckInterval), interval);

        if (_timer != null)
        {
            _timer.Period = interval.Value;
        }
    }

    public void Dispose()
    {
        _scheduledTask?.Dispose();
        _timer?.Dispose();

        GC.SuppressFinalize(this);
    }

    protected override async Task OnInitializedAsync()
    {
        DarkStart = await StorageService.GetItemAsync<TimeSpan?>(nameof(DarkStart)) ?? DarkStart;
        DarkEnd = await StorageService.GetItemAsync<TimeSpan?>(nameof(DarkEnd)) ?? DarkEnd;
        CheckInterval = await StorageService.GetItemAsync<TimeSpan?>(nameof(CheckInterval)) ?? CheckInterval;

        if (Settings is { IsManualMode: false, IsSchedule: false })
        {
            Settings.IsDarkMode = Settings.IsDarkModeSystem;
        }

        if (Settings is { IsManualMode: false, IsSchedule: true })
        {
            CheckScheduledMode();
        }

        _timer = new PeriodicTimer(CheckInterval);
        _scheduledTask = Task.Run(MonitorScheduledModeAsync);
    }

    private void UpdateState()
    {
        if (Settings is { IsManualMode: false, IsSchedule: false })
        {
            Settings.IsDarkMode = Settings.IsDarkModeSystem;

            if (_timer != null)
            {
                _timer.Period = new TimeSpan(24, 0, 0);
            }
        }

        if (Settings is { IsManualMode: false, IsSchedule: true })
        {
            CheckScheduledMode();
        }
    }

    private async Task MonitorScheduledModeAsync()
    {
        while (await _timer!.WaitForNextTickAsync())
        {
            CheckScheduledMode();
        }
    }

    private void CheckScheduledMode()
    {
        TimeSpan currentTime = DateTime.Now.TimeOfDay;

        if (DarkStart < DarkEnd)
        {
            Settings.IsDarkMode = currentTime >= DarkStart && currentTime < DarkEnd;
        }
        else
        {
            Settings.IsDarkMode = currentTime >= DarkStart || currentTime < DarkEnd;
        }

        StateHasChanged();
    }
}
