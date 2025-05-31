using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using NCalc;
using NCalc.Factories;
using System.Globalization;

namespace Money.Web.Components;

public partial class SmartSum : ComponentBase
{
    private static readonly HashSet<char> ValidSpecialCharacters = ['(', ')', '+', '-', '*', '/'];

    private bool _isNumericSumVisible = true;
    private string? _calculationSum = string.Empty;

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

    private async Task<bool> TryUpdateSumAsync()
    {
        if (_isNumericSumVisible)
        {
            return true;
        }

        decimal? sum = null;

        if (_calculationSum != null)
        {
            sum = await CalculateAsync();

            if (sum == null)
            {
                return false;
            }
        }

        Sum = sum;
        _calculationSum = Sum?.ToString(CultureInfo.CurrentCulture);

        return true;
    }

    private void UpdateSum(decimal? sum)
    {
        Sum = sum;
        _calculationSum = sum?.ToString(CultureInfo.CurrentCulture);
    }

    private async Task ToggleSumFieldAsync()
    {
        if (_isNumericSumVisible)
        {
            _calculationSum = Sum?.ToString(CultureInfo.CurrentCulture);
        }
        else if (await TryUpdateSumAsync() == false)
        {
            return;
        }

        _isNumericSumVisible = !_isNumericSumVisible;
    }

    private async Task OnSumKeyDownAsync(KeyboardEventArgs args)
    {
        var key = args.Key.Length == 1 ? args.Key[0] : '\0';

        if (key != '\0' && ValidSpecialCharacters.Contains(key))
        {
            await ToggleSumFieldAsync();
            return;
        }

        if (key != '-')
        {
            _calculationSum += key;
        }
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
            var rawSum = _calculationSum.Replace(',', '.');
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
