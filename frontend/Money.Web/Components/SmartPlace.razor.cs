using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Linq.Expressions;
using Timer = System.Timers.Timer;

namespace Money.Web.Components;

public sealed partial class SmartPlace(PlaceService placeService) : IDisposable
{
    private string? _currentText;
    private List<string> _suggestions = [];
    private bool _showSuggestions;
    private bool _isLoading;
    private int _selectedIndex = -1;
    private bool _isFocused;
    private CancellationTokenSource? _searchCancellationTokenSource;
    private Timer? _debounceTimer;

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

    public void Dispose()
    {
        _searchCancellationTokenSource?.Dispose();
        _debounceTimer?.Dispose();
        GC.SuppressFinalize(this);
    }

    protected override void OnParametersSet()
    {
        _currentText = Value;
    }

    private Task OnFocusAsync(FocusEventArgs focusEventArgs)
    {
        _ = focusEventArgs;
        _isFocused = true;
        _selectedIndex = -1;

        _debounceTimer?.Stop();
        _debounceTimer?.Dispose();
        _debounceTimer = null;

        return LoadSuggestionsAsync();
    }

    private Task OnKeyDownAsync(KeyboardEventArgs args)
    {
        if (args.Key == "ArrowDown")
        {
            if (!_showSuggestions || _suggestions.Count <= 0)
            {
                return Task.CompletedTask;
            }

            _selectedIndex = Math.Min(_selectedIndex + 1, _suggestions.Count - 1);
            StateHasChanged();
        }
        else if (args.Key == "ArrowUp")
        {
            if (!_showSuggestions || _suggestions.Count <= 0)
            {
                return Task.CompletedTask;
            }

            _selectedIndex = Math.Max(_selectedIndex - 1, -1);
            StateHasChanged();
        }
        else if (args.Key == "Enter")
        {
            if (_showSuggestions && _selectedIndex >= 0 && _selectedIndex < _suggestions.Count)
            {
                return SelectSuggestionAsync(_suggestions[_selectedIndex]);
            }

            return AcceptCurrentTextAsync();
        }
        else if (args.Key == "Escape")
        {
            _showSuggestions = false;
            _selectedIndex = -1;
            StateHasChanged();
        }
        else if (args.Key == "Tab")
        {
            if (!_showSuggestions || _suggestions.Count <= 0 || _selectedIndex < 0)
            {
                return AcceptCurrentTextAsync();
            }

            return SelectSuggestionAsync(_suggestions[_selectedIndex]);
        }

        return Task.CompletedTask;
    }

    private Task OnTextChangedAsync(string? value)
    {
        _currentText = value;
        _selectedIndex = -1;
        StartSearchDebounced();
        return Task.CompletedTask;
    }

    private void StartSearchDebounced()
    {
        if (!_isFocused)
        {
            return;
        }

        _debounceTimer?.Stop();
        _debounceTimer?.Dispose();

        _debounceTimer = new(DebounceInterval);
        _debounceTimer.Elapsed += async (_, _) =>
        {
            await InvokeAsync(LoadSuggestionsAsync);
        };

        _debounceTimer.AutoReset = false;
        _debounceTimer.Start();
    }

    private async Task LoadSuggestionsAsync()
    {
        if (_searchCancellationTokenSource != null)
        {
            await _searchCancellationTokenSource.CancelAsync();
            _searchCancellationTokenSource.Dispose();
        }

        _searchCancellationTokenSource = new();

        _isLoading = true;
        _selectedIndex = -1;
        StateHasChanged();

        try
        {
            var searchValue = _currentText;
            var places = await placeService.SearchPlace(searchValue, _searchCancellationTokenSource.Token);
            _suggestions = [.. places];
            _showSuggestions = _isFocused && _suggestions.Count > 0;
        }
        catch (OperationCanceledException)
        {
            _showSuggestions = false;
            _selectedIndex = -1;
        }
        catch (Exception)
        {
            _suggestions.Clear();
            _showSuggestions = false;
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    private async Task SelectSuggestionAsync(string suggestion)
    {
        _currentText = suggestion;
        _showSuggestions = false;
        _selectedIndex = -1;
        await ValueChanged.InvokeAsync(_currentText);
        StateHasChanged();
    }

    private async Task AcceptCurrentTextAsync()
    {
        _showSuggestions = false;
        _selectedIndex = -1;
        await ValueChanged.InvokeAsync(_currentText);
        StateHasChanged();
    }

    private async Task OnBlurAsync()
    {
        _isFocused = false;
        _debounceTimer?.Stop();
        _debounceTimer?.Dispose();
        _debounceTimer = null;

        await Task.Delay(200);
        _showSuggestions = false;
        _selectedIndex = -1;
        await ValueChanged.InvokeAsync(_currentText);
        StateHasChanged();
    }
}
