namespace Money.Web.Common;

public sealed class AppSettings
{
    public event EventHandler? OnChange;

    public bool ShowDividers
    {
        get;
        set => SetValue(ref field, value);
    }

    public bool UseChartThemeColors
    {
        get;
        set => SetValue(ref field, value);
    } = true;

    // TODO: Переделать на состояния
    public bool IsDarkMode
    {
        get;
        set => SetValue(ref field, value);
    }

    public bool IsDarkModeSystem
    {
        get;
        set => SetValue(ref field, value);
    }

    public bool IsManualMode
    {
        get;
        set => SetValue(ref field, value);
    }

    public bool IsSchedule
    {
        get;
        set => SetValue(ref field, value);
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
