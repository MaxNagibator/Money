using Microsoft.AspNetCore.Components;
using Money.ApiClient;
using System.ComponentModel.DataAnnotations;

namespace Money.Web.Components.Operations;

public partial class OperationDialog
{
    private SmartSum _smartSum = null!;
    private decimal _sum;
    private bool _isAutoFocus;

    [Parameter]
    public Operation Operation { get; set; } = null!;

    [Parameter]
    public EventCallback<Operation> OnSubmit { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    public bool IsOpen { get; private set; }

    [SupplyParameterFromForm]
    private InputModel Input { get; set; } = InputModel.Empty;

    [Inject]
    private MoneyClient MoneyClient { get; set; } = null!;

    [Inject]
    private PlaceService PlaceService { get; set; } = null!;

    [Inject]
    private CategoryService CategoryService { get; set; } = null!;

    [Inject]
    private ISnackbar SnackbarService { get; set; } = null!;

    public async Task ToggleOpen(OperationTypes.Value? type = null)
    {
        _sum = Operation.Sum;

        IsOpen = !IsOpen;

        if (IsOpen)
        {
            Input = new()
            {
                Category = Operation.Category == Category.Empty ? null : Operation.Category,
                Comment = Operation.Comment,
                Date = Operation.Date,
                Place = Operation.Place,
            };

            var categories = await CategoryService.GetAllAsync();

            // TODO: обработать, если текущая категория удалена.
            if (type == null)
            {
                Input.CategoryList = [.. categories.Where(x => x.OperationType == Operation.Category.OperationType)];
                return;
            }

            Input.CategoryList = [.. categories.Where(x => x.OperationType == type)];
        }
        else
        {
            _isAutoFocus = false;
        }

        StateHasChanged();
    }

    public Task ToggleOpen(FastOperation model)
    {
        Operation = new()
        {
            Category = model.Category,
            Sum = model.Sum,
            Comment = model.Comment,
            Place = model.Place,
            Date = Operation.Date,
        };

        _isAutoFocus = true;
        return ToggleOpen();
    }

    private async Task SubmitAsync()
    {
        try
        {
            var sum = await _smartSum.ValidateSumAsync();

            if (sum == null)
            {
                SnackbarService.Add("Нераспознано значение в поле 'сумма'.", Severity.Warning);
                return;
            }

            await SaveAsync();
            SnackbarService.Add("Успех!", Severity.Success);

            Operation.Category = Input.Category ?? throw new MoneyException("Категория операции не может быть null");
            Operation.Comment = Input.Comment;
            Operation.Date = Input.Date!.Value;
            Operation.Place = Input.Place;
            Operation.Sum = sum.Value;

            await OnSubmit.InvokeAsync(Operation);
            await ToggleOpen();
        }
        catch (Exception)
        {
            // TODO: добавить логирование ошибки
            SnackbarService.Add("Ошибка. Пожалуйста, попробуйте еще раз.", Severity.Error);
        }
    }

    private async Task SaveAsync()
    {
        var saveRequest = CreateSaveRequest();

        if (Operation.Id == null)
        {
            var result = await MoneyClient.Operations.Create(saveRequest);
            Operation.Id = result.Content;
        }
        else
        {
            await MoneyClient.Operations.Update(Operation.Id.Value, saveRequest);
        }
    }

    private OperationsClient.SaveRequest CreateSaveRequest()
    {
        return new()
        {
            CategoryId = Input.Category?.Id ?? throw new MoneyException("Идентификатор отсутствует при сохранении операции"),
            Comment = Input.Comment,
            Date = Input.Date!.Value,
            Sum = _smartSum.Sum,
            Place = Input.Place,
        };
    }

    private Task<IEnumerable<Category?>> SearchCategoryAsync(string? value, CancellationToken token)
    {
        var categories = string.IsNullOrWhiteSpace(value)
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

        public string? Comment { get; set; }

        public string? Place { get; set; }

        [Required(ErrorMessage = "Укажите дату")]
        public DateTime? Date { get; set; }
    }
}
