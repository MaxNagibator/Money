using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Linq.Expressions;

namespace Money.Web.Components;

public partial class SmartPlace(PlaceService placeService)
{
    private string? _currentText;
    private bool _isDebouncing;

    /// <summary>
    ///     <inheritdoc cref="MudAutocomplete{T}" />
    /// </summary>
    [Parameter]
    public string? Value { get; set; }

    /// <summary>
    ///     <inheritdoc cref="MudAutocomplete{T}" />
    /// </summary>
    [Parameter]
    public EventCallback<string?> ValueChanged { get; set; }

    /// <summary>
    ///     <inheritdoc cref="MudAutocomplete{T}" />
    /// </summary>
    [Parameter]
    public Expression<Func<string?>>? For { get; set; }

    /// <summary>
    ///     <inheritdoc cref="MudAutocomplete{T}" />
    /// </summary>
    [Parameter]
    public Adornment Adornment { get; set; } = Adornment.End;

    /// <summary>
    ///     <inheritdoc cref="MudAutocomplete{T}" />
    /// </summary>
    [Parameter]
    public int DebounceInterval { get; set; } = 300;

    protected override void OnParametersSet()
    {
        _currentText = Value;
    }

    private async Task<IEnumerable<string?>> SearchPlaceAsync(string? value, CancellationToken token)
    {
        var places = await placeService.SearchPlace(value, token);
        _isDebouncing = false;
        return places;
    }

    private Task OnValueChanged(string? value)
    {
        return ValueChanged.InvokeAsync(_isDebouncing ? _currentText : value);
    }

    private void OnTextChanged(string text)
    {
        _isDebouncing = true;
        _currentText = text;
    }

    private Task OnKeyDown(KeyboardEventArgs args)
    {
        if (args.Key == "Tab" && !string.IsNullOrWhiteSpace(_currentText))
        {
            return ValueChanged.InvokeAsync(_currentText);
        }

        return Task.CompletedTask;
    }
}
