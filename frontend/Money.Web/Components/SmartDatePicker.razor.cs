using Microsoft.AspNetCore.Components;
using System.Globalization;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace Money.Web.Components;

public partial class SmartDatePicker : ComponentBase
{
    private static readonly string[] DateFormats = ["d.M.yyyy", "dd.MM.yyyy", "d.MM.yyyy", "dd.M.yyyy"];

    private bool _isDatePickerVisible;
    private string? _dateText = string.Empty;
    private string? _parseError;
    private MudDatePicker? _datePicker;

    [Parameter]
    public DateTime? Date { get; set; }

    [Parameter]
    public EventCallback<DateTime?> DateChanged { get; set; }

    [Parameter]
    public Func<DateTime?>? GetInitialDate { get; set; }

    [Parameter]
    public Expression<Func<DateTime?>>? For { get; set; }

    [Inject]
    private ILogger<SmartDatePicker> Logger { get; set; } = null!;

    public async Task<DateTime?> GetDateAsync()
    {
        if (await TryUpdateDateAsync())
        {
            return Date;
        }

        return null;
    }

    protected override Task OnInitializedAsync()
    {
        return GetInitialDate != null
            ? UpdateDateAsync(GetInitialDate.Invoke())
            : Task.CompletedTask;
    }

    [GeneratedRegex("^(после)*завтра$", RegexOptions.CultureInvariant)]
    private static partial Regex FuturePattern();

    [GeneratedRegex("^(поза)*вчера$", RegexOptions.CultureInvariant)]
    private static partial Regex PastPattern();

    [GeneratedRegex("^(п)*з$", RegexOptions.CultureInvariant)]
    private static partial Regex FutureAliasPattern();

    [GeneratedRegex("^(п)*в$", RegexOptions.CultureInvariant)]
    private static partial Regex PastAliasPattern();

    private async Task<bool> TryUpdateDateAsync()
    {
        if (_isDatePickerVisible)
        {
            return true;
        }

        DateTime? date = null;

        if (_dateText != null)
        {
            date = ParseDate();

            if (date == null)
            {
                _parseError = string.IsNullOrWhiteSpace(_dateText)
                    ? null
                    : "Неверный формат даты. Используйте дд.мм.гггг, сегодня/завтра/вчера или с/з/в/пз/пв.";

                return false;
            }
        }

        _parseError = null;
        Date = date;
        _dateText = Date?.ToString("dd.MM.yyyy", CultureInfo.CurrentCulture);
        await DateChanged.InvokeAsync(Date);

        return true;
    }

    private Task UpdateDateAsync(DateTime? date)
    {
        Date = date;
        _dateText = date?.ToString("dd.MM.yyyy", CultureInfo.CurrentCulture);
        return DateChanged.InvokeAsync(Date);
    }

    private async Task OpenDatePickerAsync()
    {
        if (!string.IsNullOrWhiteSpace(_dateText) && !await TryUpdateDateAsync())
        {
            return;
        }

        _isDatePickerVisible = true;
        StateHasChanged();

        await Task.Delay(10);
        if (_datePicker != null)
        {
            await _datePicker.OpenAsync();
        }
    }

    private async Task OnPickerClosedAsync()
    {
        _dateText = Date?.ToString("dd.MM.yyyy", CultureInfo.CurrentCulture);
        await DateChanged.InvokeAsync(Date);
        _isDatePickerVisible = false;
        StateHasChanged();
    }

    private Task SwitchToTextInputAsync()
    {
        _dateText = Date?.ToString("dd.MM.yyyy", CultureInfo.CurrentCulture);
        _isDatePickerVisible = false;
        StateHasChanged();
        return Task.CompletedTask;
    }

    private Task OnTextFieldBlurAsync()
    {
        return TryUpdateDateAsync();
    }

    private DateTime? ParseDate()
    {
        DateTime? date = null;

        if (string.IsNullOrWhiteSpace(_dateText))
        {
            return date;
        }

        try
        {
            var normalizedText = _dateText.Trim().ToLowerInvariant();
            var today = DateTime.Now.Date;

            if (normalizedText is "сегодня" or "с")
            {
                return today;
            }

            var futureMatch = FuturePattern().Match(normalizedText);

            if (futureMatch.Success)
            {
                return today.AddDays(1 + futureMatch.Groups[1].Captures.Count);
            }

            var futureAliasMatch = FutureAliasPattern().Match(normalizedText);

            if (futureAliasMatch.Success)
            {
                return today.AddDays(1 + futureAliasMatch.Groups[1].Captures.Count);
            }

            var pastMatch = PastPattern().Match(normalizedText);

            if (pastMatch.Success)
            {
                return today.AddDays(-(1 + pastMatch.Groups[1].Captures.Count));
            }

            var pastAliasMatch = PastAliasPattern().Match(normalizedText);

            if (pastAliasMatch.Success)
            {
                return today.AddDays(-(1 + pastAliasMatch.Groups[1].Captures.Count));
            }

            if (DateTime.TryParseExact(normalizedText, DateFormats, CultureInfo.CurrentCulture, DateTimeStyles.None, out var parsedDate))
            {
                date = parsedDate;
            }
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "Ошибка при разборе даты. Входные данные: {DateText}", _dateText);
        }

        return date;
    }
}
