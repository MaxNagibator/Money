namespace Money.Web.Common;

public class AppSettings
{
    private bool _showDividers;
    private bool _isDarkMode;
    private bool _isDarkModeSystem;
    private bool _isManualMode;
    private bool _isSchedule;

    public event EventHandler? OnChange;

    public bool ShowDividers
    {
        get => _showDividers;
        set => SetValue(ref _showDividers, value);
    }

    // TODO: Переделать на состояния
    public bool IsDarkMode
    {
        get => _isDarkMode;
        set => SetValue(ref _isDarkMode, value);
    }

    public bool IsDarkModeSystem
    {
        get => _isDarkModeSystem;
        set => SetValue(ref _isDarkModeSystem, value);
    }

    public bool IsManualMode
    {
        get => _isManualMode;
        set => SetValue(ref _isManualMode, value);
    }

    public bool IsSchedule
    {
        get => _isSchedule;
        set => SetValue(ref _isSchedule, value);
    }

    private void SetValue(ref bool field, bool value)
    {
        if (field == value)
        {
            return;
        }

        field = value;
        OnChange?.Invoke(this, EventArgs.Empty);
    }
}
