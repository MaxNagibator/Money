using Microsoft.AspNetCore.Components;
using Money.ApiClient;
using System.ComponentModel.DataAnnotations;

namespace Money.Web.Components;

public partial class CategoryDialog
{
    private readonly DialogOptions _dialogOptions = new()
    {
        CloseButton = true,
        BackdropClick = false,
    };

    private bool _isProcessing;

    [Parameter]
    public Category Category { get; set; } = default!;

    [CascadingParameter]
    public MudDialogInstance MudDialog { get; set; } = default!;

    [SupplyParameterFromForm]
    private InputModel Input { get; set; } = default!;

    [Inject]
    private MoneyClient MoneyClient { get; set; } = default!;

    [Inject]
    private ISnackbar SnackbarService { get; set; } = default!;

    protected override void OnParametersSet()
    {
        Input = new InputModel
        {
            Name = Category.Name,
            Order = Category.Order,
            Color = Category.Color,
        };

        MudDialog.SetOptions(_dialogOptions);
    }

    private async Task SaveAsync()
    {
        _isProcessing = true;

        try
        {
            await SaveCategoryAsync();
            SnackbarService.Add("Сохранено!", Severity.Success);

            Category.Name = Input.Name;
            Category.Order = Input.Order;
            Category.Color = Input.Color;

            MudDialog.Close(DialogResult.Ok(Category));
        }
        catch (Exception)
        {
            // TODO: добавить логирование ошибки
            SnackbarService.Add("Не удалось сохранить", Severity.Error);
        }

        _isProcessing = false;
    }

    private async Task SaveCategoryAsync()
    {
        CategoryClient.SaveRequest clientCategory = CreateSaveRequest();

        if (Category.Id == null)
        {
            ApiClientResponse<int> result = await MoneyClient.Category.Create(clientCategory);
            Category.Id = result.Content;
        }
        else
        {
            await MoneyClient.Category.Update(Category.Id.Value, clientCategory);
        }
    }

    private CategoryClient.SaveRequest CreateSaveRequest()
    {
        return new CategoryClient.SaveRequest
        {
            Name = Input.Name ?? string.Empty,
            Order = Input.Order,
            Color = Input.Color,
            ParentId = Category.ParentId,
            PaymentTypeId = Category.PaymentTypeId,
        };
    }

    private void Cancel()
    {
        MudDialog.Cancel();
    }

    private sealed class InputModel
    {
        [Display(Name = "Наименование")] // todo подумать как красивее
        [Required(ErrorMessage = "Обязательно")]
        public string? Name { get; set; }

        public int? Order { get; set; }

        public string? Color { get; set; }
    }
}
