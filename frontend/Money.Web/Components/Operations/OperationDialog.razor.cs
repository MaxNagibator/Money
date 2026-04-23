using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Money.ApiClient;
using System.ComponentModel.DataAnnotations;

namespace Money.Web.Components.Operations;

public partial class OperationDialog(
    MoneyClient moneyClient,
    CategoryService categoryService,
    PlaceService placeService,
    CategoryInferenceService categoryInferenceService,
    ISnackbar snackbarService)
{
    private SmartSum _smartSum = null!;
    private SmartDatePicker _smartDatePicker = null!;
    private decimal? _sum;
    private bool _isAutoFocus;
    private EditForm? _editForm;
    private Category? _suggestedCategory;
    private bool _suggestionDismissed;
    private OperationTypes.Value _currentType = OperationTypes.None;

    [Parameter]
    public Operation Operation { get; set; } = null!;

    [Parameter]
    public EventCallback<Operation> OnSubmit { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    public bool IsOpen { get; private set; }

    [SupplyParameterFromForm]
    private InputModel Input { get; set; } = InputModel.Empty;

    public async Task ToggleOpen(OperationTypes.Value? type = null)
    {
        _sum = Operation.Sum == 0 ? null : Operation.Sum;

        IsOpen = !IsOpen;

        if (IsOpen)
        {
            _isAutoFocus = true;

            Input = new()
            {
                Category = Operation.Category == Category.Empty ? null : Operation.Category,
                Comment = Operation.Comment,
                Date = Operation.Date,
                Place = Operation.Place,
            };

            _suggestedCategory = null;
            _suggestionDismissed = false;
            _currentType = type ?? Operation.Category.OperationType;

            var categories = await categoryService.GetAllAsync();

            // TODO: обработать, если текущая категория удалена.
            Input.CategoryList = [.. categories.Where(x => x.OperationType == _currentType)];

            await TryInferCategoryAsync();
            StateHasChanged();
            return;
        }

        _isAutoFocus = false;
        _suggestedCategory = null;
        _suggestionDismissed = false;
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

        return ToggleOpen();
    }

    private async Task SubmitAsync()
    {
        if (_editForm?.EditContext?.Validate() == false)
        {
            return;
        }

        try
        {
            var sum = await _smartSum.GetSumAsync();

            if (sum == null)
            {
                snackbarService.Add("Нераспознано значение в поле 'сумма'.", Severity.Warning);
                return;
            }

            var date = await _smartDatePicker.GetDateAsync();

            if (date == null)
            {
                snackbarService.Add("Нераспознано значение в поле 'дата'.", Severity.Warning);
                return;
            }

            await SaveAsync();
            snackbarService.Add("Успех!", Severity.Success);

            if (!string.IsNullOrWhiteSpace(Input.Place) && Input.Place != Operation.Place)
            {
                placeService.InvalidateCache();
            }

            categoryInferenceService.InvalidateCache();

            Operation.Category = Input.Category ?? throw new MoneyException("Категория операции не может быть null");
            Operation.Comment = Input.Comment;
            Operation.Date = date.Value;
            Operation.Place = Input.Place;
            Operation.Sum = sum.Value;

            await OnSubmit.InvokeAsync(Operation);
            await ToggleOpen();
        }
        catch (Exception exception)
        {
            // TODO: добавить логирование ошибки
            snackbarService.Add("Ошибка. Пожалуйста, попробуйте еще раз. " + exception.Message, Severity.Error);
        }
    }

    private async Task SaveAsync()
    {
        var saveRequest = CreateSaveRequest();

        if (Operation.Id == null)
        {
            var result = await moneyClient.Operations.Create(saveRequest);
            Operation.Id = result.Content;
        }
        else
        {
            await moneyClient.Operations.Update(Operation.Id.Value, saveRequest);
        }
    }

    private OperationsClient.SaveRequest CreateSaveRequest()
    {
        return new()
        {
            CategoryId = Input.Category?.Id ?? throw new MoneyException("Идентификатор отсутствует при сохранении операции"),
            Comment = Input.Comment,
            Date = DateTime.SpecifyKind(_smartDatePicker.Date!.Value.Date, DateTimeKind.Unspecified),
            Sum = _smartSum.Sum ?? 0,
            Place = Input.Place,
        };
    }

    private async Task OnPlaceCommittedAsync()
    {
        if (Input.Category != null || _suggestionDismissed)
        {
            return;
        }

        await TryInferCategoryAsync();
        StateHasChanged();
    }

    private async Task TryInferCategoryAsync()
    {
        if (_currentType.Id == 0 || string.IsNullOrWhiteSpace(Input.Place) || Input.Category != null)
        {
            _suggestedCategory = null;
            return;
        }

        _suggestedCategory = await categoryInferenceService.InferAsync(Input.Place, _currentType);

        if (_suggestedCategory != null)
        {
            Input.Category = _suggestedCategory;
        }
    }

    private void ClearSuggestion()
    {
        Input.Category = null;
        _suggestedCategory = null;
        _suggestionDismissed = true;
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
