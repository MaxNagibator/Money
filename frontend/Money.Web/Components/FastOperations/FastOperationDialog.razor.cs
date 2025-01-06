using Microsoft.AspNetCore.Components;
using Money.ApiClient;
using System.ComponentModel.DataAnnotations;

namespace Money.Web.Components.FastOperations;

public partial class FastOperationDialog
{
    private readonly DialogOptions _dialogOptions = new()
    {
        CloseButton = true,
        BackdropClick = false,
    };

    private SmartSum _smartSum = null!;

    private bool _isProcessing;

    [CascadingParameter]
    public MudDialogInstance MudDialog { get; set; } = null!;

    [Parameter]
    public Category Category { get; set; } = null!;

    [Parameter]
    public FastOperation FastOperation { get; set; } = null!;

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

    protected override async Task OnParametersSetAsync()
    {
        Input = new()
        {
            Category = FastOperation.Category == Category.Empty ? null : FastOperation.Category,
            Comment = FastOperation.Comment,
            Name = FastOperation.Name,
            Order = FastOperation.Order,
            Place = FastOperation.Place,
        };

        MudDialog.SetOptions(_dialogOptions);

        var categories = await CategoryService.GetAllAsync();

        if (Input.Category == null)
        {
            Input.CategoryList = [.. categories];
        }
        else
        {
            Input.CategoryList = [.. categories.Where(x => x.OperationType == FastOperation.Category.OperationType)];
        }
    }

    private async Task SubmitAsync()
    {
        _isProcessing = true;

        try
        {
            var sum = await _smartSum.ValidateSumAsync();

            if (sum == null)
            {
                _isProcessing = false;
                SnackbarService.Add("Нераспознано значение в поле 'сумма'.", Severity.Warning);
                return;
            }

            await SaveAsync();
            SnackbarService.Add("Успех!", Severity.Success);

            FastOperation.Name = Input.Name!;
            FastOperation.Category = Input.Category ?? throw new MoneyException("Категория операции не может быть null");
            FastOperation.Sum = sum.Value;
            FastOperation.Comment = Input.Comment;
            FastOperation.Place = Input.Place;
            FastOperation.Order = Input.Order;

            MudDialog.Close(DialogResult.Ok(FastOperation));
        }
        catch (Exception)
        {
            // TODO: добавить логирование ошибки
            SnackbarService.Add("Ошибка. Пожалуйста, попробуйте еще раз.", Severity.Error);
        }

        _isProcessing = false;
    }

    private async Task SaveAsync()
    {
        var saveRequest = CreateSaveRequest();

        if (FastOperation.Id == null)
        {
            var result = await MoneyClient.FastOperation.Create(saveRequest);
            FastOperation.Id = result.Content;
        }
        else
        {
            await MoneyClient.FastOperation.Update(FastOperation.Id.Value, saveRequest);
        }
    }

    private FastOperationClient.SaveRequest CreateSaveRequest()
    {
        return new()
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
        var categories = string.IsNullOrWhiteSpace(value)
            ? Input.CategoryList
            : Input.CategoryList?.Where(x => x.Name.Contains(value, StringComparison.InvariantCultureIgnoreCase));

        return Task.FromResult(categories ?? [])!;
    }

    private Task<IEnumerable<string?>> SearchPlaceAsync(string? value, CancellationToken token)
    {
        return PlaceService.SearchPlace(value, token)!;
    }

    private void Cancel()
    {
        MudDialog.Cancel();
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
