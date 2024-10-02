using Microsoft.AspNetCore.Components;
using Money.ApiClient;

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

    [Inject]
    private MoneyClient MoneyClient { get; set; } = default!;

    [Inject]
    private ISnackbar SnackbarService { get; set; } = default!;

    protected override void OnParametersSet()
    {
        MudDialog.SetOptions(_dialogOptions);
    }

    private async Task SaveAsync()
    {
        _isProcessing = true;

        try
        {
            await SaveCategoryAsync();
            SnackbarService.Add("Сохранено!", Severity.Success);
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
            Name = Category.Name ?? string.Empty,
            PaymentTypeId = Category.PaymentTypeId,
            Color = Category.Color,
            Order = Category.Order,
            ParentId = Category.ParentId,
        };
    }

    private void Cancel()
    {
        // todo исходный объект не трогать, если отмена.
        MudDialog.Cancel();
    }
}
