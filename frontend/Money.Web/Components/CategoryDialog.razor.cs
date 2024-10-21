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

    private async Task SubmitAsync()
    {
        _isProcessing = true;

        try
        {
            await SaveAsync();
            SnackbarService.Add("Категория успешно сохранена!", Severity.Success);

            Category.Name = Input.Name;
            Category.Order = Input.Order;
            Category.Color = Input.Color;

            MudDialog.Close(DialogResult.Ok(Category));
        }
        catch (Exception)
        {
            // TODO: добавить логирование ошибки
            SnackbarService.Add("Не удалось сохранить категорию. Пожалуйста, попробуйте еще раз.", Severity.Error);
        }

        _isProcessing = false;
    }

    private async Task SaveAsync()
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
            Name = Input.Name,
            Order = Input.Order,
            Color = Input.Color,
            ParentId = Category.ParentId,
            PaymentTypeId = Category.PaymentType.Id,
        };
    }

    private void Cancel()
    {
        MudDialog.Cancel();
    }

    private sealed class InputModel
    {
        [Display(Name = "Наименование")] // todo подумать как красивее
        [Required(ErrorMessage = "Необходимо указать наименование")]
        public required string Name { get; set; }

        public int? Order { get; set; }

        public string? Color { get; set; }
    }
}
