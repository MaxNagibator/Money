using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Money.ApiClient;
using NCalc;
using NCalc.Factories;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Money.Web.Components.Operations;

public partial class OperationDialog
{
    private static readonly HashSet<char> ValidKeys = ['(', ')', '+', '-', '*', '/'];
    private static readonly Dictionary<string, List<string>> Cache = new();

    private bool _isNumericSumVisible = true;

    [CascadingParameter]
    public List<Category> Categories { get; set; } = default!;

    [Parameter]
    public Operation Operation { get; set; } = default!;

    [Parameter]
    public EventCallback<Operation> OnSubmit { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    public bool IsOpen { get; private set; }

    [SupplyParameterFromForm]
    private InputModel Input { get; set; } = InputModel.Empty;

    [Inject]
    private MoneyClient MoneyClient { get; set; } = default!;

    [Inject]
    private PlaceService PlaceService { get; set; } = default!;

    [Inject]
    private ISnackbar SnackbarService { get; set; } = default!;

    [Inject]
    private IAsyncExpressionFactory Factory { get; set; } = default!;

    public override Task SetParametersAsync(ParameterView parameters)
    {
        foreach (ParameterValue parameter in parameters)
        {
            switch (parameter.Name)
            {
                case nameof(Categories):
                    Categories = (List<Category>)parameter.Value;
                    break;

                case nameof(Operation):
                    Operation = (Operation)parameter.Value;
                    break;

                case nameof(OnSubmit):
                    OnSubmit = (EventCallback<Operation>)parameter.Value;
                    break;

                case nameof(ChildContent):
                    ChildContent = (RenderFragment?)parameter.Value;
                    break;

                default:
                    throw new ArgumentException($"Unknown parameter: {parameter.Name}");
            }
        }

        return base.SetParametersAsync(ParameterView.Empty);
    }

    public void ToggleOpen(OperationTypes.Value? type = null)
    {
        IsOpen = !IsOpen;

        if (IsOpen == false)
        {
            return;
        }

        Input = new InputModel
        {
            Category = Operation.Category == Category.Empty ? null : Operation.Category,
            Comment = Operation.Comment,
            Date = Operation.Date,
            Place = Operation.Place,
            Sum = Operation.Sum,
            CalculationSum = Operation.Sum.ToString(CultureInfo.CurrentCulture),
        };

        // todo обработать, если текущая категория удалена.
        if (type == null)
        {
            Input.CategoryList = [.. Categories.Where(x => x.OperationType == Operation.Category.OperationType)];
            return;
        }

        Input.CategoryList = [.. Categories.Where(x => x.OperationType == type)];
    }

    private async Task ToggleSumFieldAsync()
    {
        if (_isNumericSumVisible)
        {
            Input.CalculationSum = Input.Sum.ToString(CultureInfo.CurrentCulture);
        }
        else if (await ValidateSumAsync() == false)
        {
            return;
        }

        _isNumericSumVisible = !_isNumericSumVisible;
    }

    private async Task<bool> ValidateSumAsync()
    {
        if (_isNumericSumVisible)
        {
            return true;
        }

        decimal? sum = await CalculateAsync();

        if (sum == null)
        {
            return false;
        }

        Input.Sum = sum.Value;
        Input.CalculationSum = Input.Sum.ToString(CultureInfo.CurrentCulture);

        return true;
    }

    private async Task<decimal?> CalculateAsync()
    {
        decimal? sum = null;

        if (string.IsNullOrWhiteSpace(Input.CalculationSum))
        {
            return sum;
        }

        try
        {
            string rawSum = Input.CalculationSum.Replace(',', '.');
            AsyncExpression expression = Factory.Create(rawSum, ExpressionOptions.DecimalAsDefault);

            object? rawResult = await expression.EvaluateAsync();
            sum = Convert.ToDecimal(rawResult);
        }
        catch (Exception)
        {
            SnackbarService.Add("Нераспознано значение в поле 'сумма'.", Severity.Warning);
        }

        return sum;
    }

    private async Task OnSumKeyDownAsync(KeyboardEventArgs args)
    {
        char key = args.Key.Length == 1 ? args.Key[0] : '\0';

        if (key == '\0' || !ValidKeys.Contains(key))
        {
            return;
        }

        await ToggleSumFieldAsync();

        // Костыль, но ‘-’ валидный символ для NumericField, поэтому происходит его повторное добавление
        // InputMode.@decimal / https://developer.mozilla.org/ru/docs/Web/HTML/Global_attributes/inputmode#decimal
        if (key != '-')
        {
            Input.CalculationSum += key;
        }
    }

    private async Task SubmitAsync()
    {
        try
        {
            if (await ValidateSumAsync() == false)
            {
                return;
            }

            await SaveAsync();
            SnackbarService.Add("Операция успешно сохранена!", Severity.Success);

            Operation.Category = Input.Category ?? throw new MoneyException("Категория операции не может быть null");
            Operation.Comment = Input.Comment;
            Operation.Date = Input.Date!.Value;
            Operation.Place = Input.Place;
            Operation.Sum = Input.Sum;

            await OnSubmit.InvokeAsync(Operation);
            ToggleOpen();
        }
        catch (Exception)
        {
            // TODO: добавить логирование ошибки
            SnackbarService.Add("Не удалось сохранить операцию. Пожалуйста, попробуйте еще раз.", Severity.Error);
        }
    }

    private async Task SaveAsync()
    {
        OperationClient.SaveRequest clientCategory = CreateSaveRequest();

        if (Operation.Id == null)
        {
            ApiClientResponse<int> result = await MoneyClient.Operation.Create(clientCategory);
            Operation.Id = result.Content;
        }
        else
        {
            await MoneyClient.Operation.Update(Operation.Id.Value, clientCategory);
        }
    }

    private OperationClient.SaveRequest CreateSaveRequest()
    {
        return new OperationClient.SaveRequest
        {
            CategoryId = Input.Category?.Id ?? throw new MoneyException("Идентификатор категории отсутствует при сохранении операции"),
            Comment = Input.Comment,
            Date = Input.Date!.Value,
            Sum = Input.Sum,
            Place = Input.Place,
        };
    }

    private Task<IEnumerable<Category?>> SearchCategoryAsync(string? value, CancellationToken token)
    {
        IEnumerable<Category>? categories = string.IsNullOrWhiteSpace(value)
            ? Input.CategoryList
            : Input.CategoryList?.Where(x => x.Name.Contains(value, StringComparison.InvariantCultureIgnoreCase));

        return Task.FromResult(categories ?? [])!;
    }

    private Task<IEnumerable<string?>> SearchPlaceAsync(string? value, CancellationToken token)
    {
        return PlaceService.SearchPlace(value, token)!;
    }

    private sealed class InputModel
    {
        public static readonly InputModel Empty = new()
        {
            Category = Category.Empty,
        };

        [Required(ErrorMessage = "Категория обязательна")]
        public Category? Category { get; set; }

        public List<Category>? CategoryList { get; set; }

        [Required(ErrorMessage = "Требуется сумма")]
        [Range(double.MinValue, double.MaxValue, ErrorMessage = "Сумма вне допустимого диапазона")]
        public decimal Sum { get; set; }

        public string? CalculationSum { get; set; }

        public string? Comment { get; set; }

        public string? Place { get; set; }

        [Required(ErrorMessage = "Укажите дату")]
        public DateTime? Date { get; set; }
    }
}
