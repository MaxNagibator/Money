using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Money.ApiClient;

namespace Money.Web.Pages;

public partial class CategoryDialog
{
    private readonly DialogOptions _dialogOptions = new()
    {
        CloseButton = true,
        BackdropClick = false
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

    [Inject]
    private ILocalStorageService LocalStorage { get; set; } = default!;

    protected override void OnParametersSet()
    {
        MudDialog.SetOptions(_dialogOptions);
    }

    private async Task SaveAsync()
    {
        _isProcessing = true;

        try
        {
            if(Category.Id == null)
            {
                var result = await MoneyClient.Category.Create(new CategoryClient.CreateCategoryRequest
                {
                    Name = Category.Name,
                    PaymentTypeId = Category.PaymentTypeId,
                    Color = Category.Color,
                    Order = Category.Order,
                    ParentId = Category.ParentId,
                });
                Category.Id = result.Content;
            }
            else
            {
                await MoneyClient.Category.Update(new CategoryClient.UpdateCategoryRequest
                {
                    Id = Category.Id.Value,
                    Name = Category.Name,
                    PaymentTypeId = Category.PaymentTypeId,
                    Color = Category.Color,
                    Order = Category.Order,
                    ParentId = Category.ParentId,
                });
            }
            SnackbarService.Add("Сохранено!", Severity.Success);
            MudDialog.Close(DialogResult.Ok(Category));
        }
        catch
        {
            // todo лог
            SnackbarService.Add("Не удалось сохранить", Severity.Error);
        }

        _isProcessing = false;
    }

    private void Cancel()
    {
        MudDialog.Cancel();
    }
}
