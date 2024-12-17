using Microsoft.AspNetCore.Components;
using Money.ApiClient;
using System.ComponentModel.DataAnnotations;

namespace Money.Web.Components.FastOperations;

public partial class FastOperationDialog
{
    private SmartSum _smartSum = null!;

    [CascadingParameter]
    public List<Category> Categories { get; set; } = null!;

    [Parameter]
    public FastOperation FastOperation { get; set; } = null!;

    [Parameter]
    public EventCallback<FastOperation> OnSubmit { get; set; }

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
    private ISnackbar SnackbarService { get; set; } = null!;

    public void ToggleOpen(OperationTypes.Value? type = null)
    {
        IsOpen = !IsOpen;

        if (IsOpen == false)
        {
            return;
        }

        Input = new InputModel
        {
            Category = FastOperation.Category == Category.Empty ? null : FastOperation.Category,
            Comment = FastOperation.Comment,
            Name = FastOperation.Name,
            Order = FastOperation.Order,
            Place = FastOperation.Place,
        };

        // todo обработать, если текущая категория удалена.
        if (type == null)
        {
            Input.CategoryList = [.. Categories.Where(x => x.OperationType == FastOperation.Category.OperationType)];
            return;
        }

        Input.CategoryList = [.. Categories.Where(x => x.OperationType == type)];

        _smartSum.UpdateSum(FastOperation.Sum);
    }

    private async Task SubmitAsync()
    {
        try
        {
            decimal? sum = await _smartSum.ValidateSumAsync();

            if (sum == null)
            {
                SnackbarService.Add("Нераспознано значение в поле 'сумма'.", Severity.Warning);
                return;
            }

            await SaveAsync();
            SnackbarService.Add("Успех!", Severity.Success);

            FastOperation.Category = Input.Category ?? throw new MoneyException("Категория операции не может быть null");
            FastOperation.Comment = Input.Comment;
            FastOperation.Name = Input.Name!;
            FastOperation.Name = Input.Name!;
            FastOperation.Place = Input.Place;
            FastOperation.Sum = sum.Value;

            await OnSubmit.InvokeAsync(FastOperation);
            ToggleOpen();
        }
        catch (Exception)
        {
            // TODO: добавить логирование ошибки
            SnackbarService.Add("Ошибка. Пожалуйста, попробуйте еще раз.", Severity.Error);
        }
    }

    private async Task SaveAsync()
    {
        FastOperationClient.SaveRequest saveRequest = CreateSaveRequest();

        if (FastOperation.Id == null)
        {
            ApiClientResponse<int> result = await MoneyClient.FastOperation.Create(saveRequest);
            FastOperation.Id = result.Content;
        }
        else
        {
            await MoneyClient.FastOperation.Update(FastOperation.Id.Value, saveRequest);
        }
    }

    private FastOperationClient.SaveRequest CreateSaveRequest()
    {
        return new FastOperationClient.SaveRequest
        {
            CategoryId = Input.Category?.Id ?? throw new MoneyException("Идентификатор отсутствует при сохранении операции"),
            Comment = Input.Comment,
            Name = Input.Name!,
            Sum = _smartSum.Sum,
            Place = Input.Place,
            Order = Input.Order,
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

        [Required(ErrorMessage = "Заполни меня")]
        public Category? Category { get; set; }

        public List<Category>? CategoryList { get; set; }

        [Required(ErrorMessage = "Заполни меня")]
        public string? Name { get; set; }

        public string? Comment { get; set; }

        public string? Place { get; set; }

        public int? Order { get; set; }
    }
}
