using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Money.ApiClient;
using System.ComponentModel.DataAnnotations;

namespace Money.Web.Components.RegularOperations;

public partial class RegularOperationDialog
{
    private readonly DialogOptions _dialogOptions = new()
    {
        CloseButton = true,
        BackdropClick = false,
    };

    private SmartSum _smartSum = null!;

    private bool _isProcessing;
    private EditForm? _editForm;

    [CascadingParameter]
    public IMudDialogInstance MudDialog { get; set; } = null!;

    [Parameter]
    public RegularOperation Model { get; set; } = null!;

    [Parameter]
    public OperationTypes.Value? RequiredType { get; set; }

    [SupplyParameterFromForm]
    private InputModel Input { get; set; } = InputModel.Empty;

    [Inject]
    private MoneyClient MoneyClient { get; set; } = null!;

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
            Place = Model.Place,
            DateFrom = Model.DateFrom,
            DateTo = Model.DateTo,
            TimeType = Model.TimeType,
            TimeValue = Model.TimeValue,
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
        if (_editForm?.EditContext?.Validate() == false)
        {
            return;
        }

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

            Model.Name = Input.Name;
            Model.Category = Input.Category ?? throw new MoneyException("Категория операции не может быть null");
            Model.Sum = sum.Value;
            Model.Comment = Input.Comment;
            Model.Place = Input.Place;
            Model.DateFrom = Input.DateFrom!.Value;
            Model.DateTo = Input.DateTo;
            Model.TimeType = Input.TimeType;
            Model.TimeValue = Input.TimeValue;

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
            var result = await MoneyClient.RegularOperations.Create(saveRequest);
            Model.Id = result.Content;
        }
        else
        {
            await MoneyClient.RegularOperations.Update(Model.Id.Value, saveRequest);
        }

        var getOperations = await MoneyClient.RegularOperations.GetById(Model.Id.Value);
        Model.RunTime = getOperations.Content!.RunTime;
    }

    private RegularOperationsClient.SaveRequest CreateSaveRequest()
    {
        return new()
        {
            CategoryId = Input.Category?.Id ?? throw new MoneyException("Идентификатор отсутствует при сохранении операции"),
            Comment = Input.Comment,
            Name = Input.Name,
            Sum = _smartSum.Sum ?? 0,
            Place = Input.Place,
            DateFrom = Input.DateFrom!.Value,
            DateTo = Input.DateTo,
            TimeTypeId = Input.TimeType.Id,
            TimeValue = Input.IsTimeValueAvailable ? Input.TimeValue : null,
        };
    }

    private Task<IEnumerable<Category?>> SearchCategoryAsync(string? value, CancellationToken token)
    {
        var categories = string.IsNullOrWhiteSpace(value)
            ? Input.CategoryList
            : Input.CategoryList?.Where(x => x.Name.Contains(value, StringComparison.InvariantCultureIgnoreCase));

        return Task.FromResult(categories ?? [])!;
    }

    private void Cancel()
    {
        MudDialog.Cancel();
    }

    private sealed class InputModel : IValidatableObject
    {
        public static readonly InputModel Empty = new()
        {
            Name = "Незаполнен",
            Category = Category.Empty,
            TimeType = RegularOperationTimeTypes.None,
        };

        [Required(ErrorMessage = "Заполни меня")]
        public Category? Category { get; set; }

        public List<Category>? CategoryList { get; set; }

        [Required(ErrorMessage = "Заполни меня")]
        public required string Name { get; set; }

        public string? Comment { get; set; }

        public string? Place { get; set; }

        [Required(ErrorMessage = "Заполни меня")]
        public required RegularOperationTimeTypes.Value TimeType { get; set; }

        public int? TimeValue { get; set; }

        [Required(ErrorMessage = "Заполни меня")]
        public DateTime? DateFrom { get; set; }

        public DateTime? DateTo { get; set; }

        public bool IsTimeValueAvailable => TimeType != RegularOperationTimeTypes.Values.First().Value;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (IsTimeValueAvailable && TimeValue == null)
            {
                yield return new("Заполни меня", [nameof(TimeValue)]);
            }
        }
    }
}
