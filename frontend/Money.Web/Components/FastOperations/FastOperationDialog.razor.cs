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
    public IMudDialogInstance MudDialog { get; set; } = null!;

    [Parameter]
    public FastOperation Model { get; set; } = null!;

    [Parameter]
    public OperationTypes.Value? RequiredType { get; set; }

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
            Category = Model.Category == Category.Empty ? null : Model.Category,
            Comment = Model.Comment,
            Name = Model.Name,
            Order = Model.Order,
            Place = Model.Place,
        };

        var categories = await CategoryService.GetAllAsync();

        if (RequiredType != null)
        {
            Input.CategoryList = [.. categories.Where(x => x.OperationType == RequiredType)];
        }
        else if (Input.Category != null)
        {
            Input.CategoryList = [.. categories.Where(x => x.OperationType == Model.Category.OperationType)];
        }
        else
        {
            Input.CategoryList = [.. categories];
        }

        await MudDialog.SetOptionsAsync(_dialogOptions);
    }

    private async Task SubmitAsync()
    {
        _isProcessing = true;

        try
        {
            var sum = await _smartSum.GetSumAsync();

            if (sum == null)
            {
                _isProcessing = false;
                SnackbarService.Add("Нераспознано значение в поле 'сумма'.", Severity.Warning);
                return;
            }

            await SaveAsync();
            SnackbarService.Add("Успех!", Severity.Success);

            Model.Name = Input.Name!;
            Model.Category = Input.Category ?? throw new MoneyException("Категория операции не может быть null");
            Model.Sum = sum.Value;
            Model.Comment = Input.Comment;
            Model.Place = Input.Place;
            Model.Order = Input.Order;

            MudDialog.Close(DialogResult.Ok(Model));
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

        if (Model.Id == null)
        {
            var result = await MoneyClient.FastOperations.Create(saveRequest);
            Model.Id = result.Content;
        }
        else
        {
            await MoneyClient.FastOperations.Update(Model.Id.Value, saveRequest);
        }
    }

    private FastOperationsClient.SaveRequest CreateSaveRequest()
    {
        return new()
        {
            CategoryId = Input.Category?.Id ?? throw new MoneyException("Идентификатор отсутствует при сохранении операции"),
            Comment = Input.Comment,
            Name = Input.Name!,
            Sum = _smartSum.Sum ?? 0,
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
