using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using NCalc;
using NCalc.Factories;
using System.Globalization;

namespace Money.Web.Components;

public partial class SmartSum : ComponentBase
{
    private static readonly HashSet<char> ValidSpecialCharacters = ['(', ')', '+', '-', '*', '/', '.', ','];

    private bool _isNumericSumVisible = true;
    private string? _calculationSum = string.Empty;
    private MudNumericField<decimal?>? _numericField;

    public decimal? Sum { get; set; }

    // TODO: Костыль для корректной работы Popover
    [Parameter]
    public Func<decimal?>? GetInitialSum { get; set; }

    [Parameter]
    public bool IsAutoFocus { get; set; }

    [Inject]
    private IAsyncExpressionFactory Factory { get; set; } = null!;

    [Inject]
    private ILogger<SmartSum> Logger { get; set; } = null!;

    private bool HasExpression
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_calculationSum))
            {
                return false;
            }

            var normalized = string.Concat(_calculationSum.Where(c => !char.IsWhiteSpace(c))).Replace(',', '.');
            return !decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out _);
        }
    }

    public async Task<decimal?> GetSumAsync()
    {
        if (await TryUpdateSumAsync())
        {
            return Sum;
        }

        return null;
    }

    protected override void OnInitialized()
    {
        if (GetInitialSum != null)
        {
            UpdateSum(GetInitialSum.Invoke());
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender || _numericField == null)
        {
            return;
        }

        if (Sum.HasValue)
        {
            _numericField.ForceRender(true);
            StateHasChanged();
        }

        if (IsAutoFocus)
        {
            await _numericField.FocusAsync();
        }
    }

    private async Task<bool> TryUpdateSumAsync()
    {
        if (_isNumericSumVisible)
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(_calculationSum))
        {
            return true;
        }

        var sum = await CalculateAsync();

        if (sum == null)
        {
            return false;
        }

        Sum = sum;
        return true;
    }

    private void UpdateSum(decimal? sum)
    {
        Sum = sum;
        _calculationSum = sum?.ToString(CultureInfo.CurrentCulture);
    }

    private async Task EnterTextModeAsync()
    {
        if (!_isNumericSumVisible)
        {
            return;
        }

        if (!HasExpression)
        {
            _calculationSum = Sum?.ToString(CultureInfo.CurrentCulture);
        }

        _isNumericSumVisible = false;
        await Task.Delay(10);
        StateHasChanged();
    }

    private async Task ExitTextModeAsync()
    {
        if (_isNumericSumVisible)
        {
            return;
        }

        if (!await TryUpdateSumAsync())
        {
            return;
        }

        _isNumericSumVisible = true;
        await Task.Delay(10);
        StateHasChanged();
    }

    private Task OnNumericFocusInAsync()
    {
        if (!_isNumericSumVisible || !HasExpression)
        {
            return Task.CompletedTask;
        }

        return EnterTextModeAsync();
    }

    private Task OnSumKeyDownAsync(KeyboardEventArgs args)
    {
        if (!_isNumericSumVisible || HasExpression)
        {
            return Task.CompletedTask;
        }

        var key = args.Key.Length == 1 ? args.Key[0] : '\0';

        if (key == '\0' || !ValidSpecialCharacters.Contains(key))
        {
            return Task.CompletedTask;
        }

        return EnterTextModeAsync();
    }

    private async Task<decimal?> CalculateAsync()
    {
        decimal? sum = null;

        if (string.IsNullOrWhiteSpace(_calculationSum))
        {
            return sum;
        }

        try
        {
            var rawSum = string.Concat(_calculationSum.Where(c => !char.IsWhiteSpace(c))).Replace(',', '.');
            var expression = Factory.Create(rawSum, ExpressionOptions.DecimalAsDefault);

            var rawResult = await expression.EvaluateAsync();

            if (rawResult is decimal result)
            {
                sum = result;
            }
            else if (rawResult != null && decimal.TryParse(rawResult.ToString(), out var parsedResult))
            {
                sum = parsedResult;
            }
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "Ошибка при вычислении суммы. Входные данные: {CalculationSum}", _calculationSum);
        }

        return sum;
    }
}
