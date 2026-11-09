using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Linq.Expressions;

namespace Money.Web.Components;

public sealed partial class SmartCategory(IJSRuntime jsRuntime) : IDisposable
{
    private readonly string _id = "sc-" + Guid.NewGuid().ToString("N");
    private string? _currentText;
    private List<Category> _suggestions = [];
    private bool _showSuggestions;
    private int _selectedIndex = -1;
    private int _scrollToIndex = -1;
    private bool _isFocused;
    private FieldIdentifier _fieldIdentifier;
    private bool _hasError;
    private string? _errorText;
    private EditContext? _subscribedEditContext;

    [Parameter]
    public Category? Value { get; set; }

    [Parameter]
    public EventCallback<Category?> ValueChanged { get; set; }

    [Parameter]
    public Expression<Func<Category?>>? For { get; set; }

    [Parameter]
    public List<Category> Options { get; set; } = [];

    [Parameter]
    public Adornment Adornment { get; set; } = Adornment.Start;

    [CascadingParameter]
    private EditContext? EditContext { get; set; }

    public void Dispose()
    {
        UnsubscribeEditContext();
        GC.SuppressFinalize(this);
    }

    protected override void OnParametersSet()
    {
        _currentText = Value?.Name;

        if (For != null && EditContext != null)
        {
            var identifier = FieldIdentifier.Create(For);

            if (identifier.Equals(_fieldIdentifier) && ReferenceEquals(EditContext, _subscribedEditContext))
            {
                return;
            }

            UnsubscribeEditContext();
            _fieldIdentifier = identifier;
            _subscribedEditContext = EditContext;
            EditContext.OnValidationStateChanged += OnValidationStateChanged;
            UpdateValidationState();
        }
        else
        {
            UnsubscribeEditContext();
            _hasError = false;
            _errorText = null;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_scrollToIndex >= 0)
        {
            var index = _scrollToIndex;
            _scrollToIndex = -1;
            await jsRuntime.InvokeVoidAsync("moneyUi.scrollIntoView", GetItemId(index));
        }
    }

    private void OnValidationStateChanged(object? sender, ValidationStateChangedEventArgs args)
    {
        _ = args;
        UpdateValidationState();
        StateHasChanged();
    }

    private string GetItemId(int index)
    {
        return $"{_id}-item-{index}";
    }

    private void UpdateValidationState()
    {
        if (_subscribedEditContext == null)
        {
            _hasError = false;
            _errorText = null;
            return;
        }

        var messages = _subscribedEditContext.GetValidationMessages(_fieldIdentifier).ToArray();
        _hasError = messages.Length > 0;
        _errorText = _hasError ? messages[0] : null;
    }

    private void UnsubscribeEditContext()
    {
        if (_subscribedEditContext == null)
        {
            return;
        }

        _subscribedEditContext.OnValidationStateChanged -= OnValidationStateChanged;
        _subscribedEditContext = null;
    }

    private Task OnFocusAsync(FocusEventArgs args)
    {
        _isFocused = true;
        _selectedIndex = -1;
        UpdateSuggestions(_currentText);
        return Task.CompletedTask;
    }

    private Task OnKeyDownAsync(KeyboardEventArgs args)
    {
        switch (args.Key)
        {
            case "ArrowDown":
                if (_suggestions.Count == 0)
                {
                    return Task.CompletedTask;
                }

                _showSuggestions = true;
                _selectedIndex = Math.Min(_selectedIndex + 1, _suggestions.Count - 1);
                _scrollToIndex = _selectedIndex;
                StateHasChanged();
                break;

            case "ArrowUp":
                if (_suggestions.Count == 0)
                {
                    return Task.CompletedTask;
                }

                _showSuggestions = true;
                _selectedIndex = Math.Max(_selectedIndex - 1, -1);
                _scrollToIndex = _selectedIndex;
                StateHasChanged();
                break;

            case "Enter":
                if (_showSuggestions && _selectedIndex >= 0 && _selectedIndex < _suggestions.Count)
                {
                    return SelectSuggestionAsync(_suggestions[_selectedIndex]);
                }

                break;

            case "Escape":
                _showSuggestions = false;
                _selectedIndex = -1;
                StateHasChanged();
                break;

            case "Tab":
                if (_showSuggestions && _suggestions.Count > 0 && _selectedIndex >= 0)
                {
                    return SelectSuggestionAsync(_suggestions[_selectedIndex]);
                }

                _showSuggestions = false;
                _selectedIndex = -1;
                StateHasChanged();
                break;
        }

        return Task.CompletedTask;
    }

    private async Task OnTextChangedAsync(string? value)
    {
        _currentText = value;
        _selectedIndex = -1;

        if (string.IsNullOrEmpty(value) && Value != null)
        {
            await ValueChanged.InvokeAsync(null);
            NotifyFieldChanged();
        }

        UpdateSuggestions(value);
        StateHasChanged();
    }

    private void UpdateSuggestions(string? filter)
    {
        _suggestions = string.IsNullOrWhiteSpace(filter)
            ? [.. Options]
            : [.. Options.Where(x => x.Name.Contains(filter, StringComparison.InvariantCultureIgnoreCase))];

        _showSuggestions = _isFocused && _suggestions.Count > 0;
    }

    private async Task SelectSuggestionAsync(Category suggestion)
    {
        _currentText = suggestion.Name;
        _showSuggestions = false;
        _selectedIndex = -1;
        await ValueChanged.InvokeAsync(suggestion);
        NotifyFieldChanged();
        StateHasChanged();
    }

    private async Task OnBlurAsync()
    {
        _isFocused = false;
        await Task.Delay(200);

        _showSuggestions = false;
        _selectedIndex = -1;

        if (string.IsNullOrEmpty(_currentText))
        {
            if (Value != null)
            {
                await ValueChanged.InvokeAsync(null);
                NotifyFieldChanged();
            }
        }
        else if (Value == null || !string.Equals(_currentText, Value.Name, StringComparison.Ordinal))
        {
            _currentText = Value?.Name;
        }

        StateHasChanged();
    }

    private void NotifyFieldChanged()
    {
        if (_subscribedEditContext != null && !_fieldIdentifier.Equals(default))
        {
            _subscribedEditContext.NotifyFieldChanged(_fieldIdentifier);
        }
    }
}
