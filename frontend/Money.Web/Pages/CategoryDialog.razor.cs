using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;

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
    private ISnackbar SnackbarService { get; set; } = default!;

    [Inject]
    private ILocalStorageService LocalStorage { get; set; } = default!;

    protected override void OnParametersSet()
    {
        MudDialog.SetOptions(_dialogOptions);
    }

    private async Task UpdateAsync()
    {
        _isProcessing = true;

        try
        {
            SnackbarService.Add("Сохранено!", Severity.Success);
            MudDialog.Close();
        }
        catch
        {
            SnackbarService.Add("Не удалось сохранить", Severity.Error);
        }

        _isProcessing = false;
    }

    private void Cancel()
    {
        MudDialog.Cancel();
    }
}
