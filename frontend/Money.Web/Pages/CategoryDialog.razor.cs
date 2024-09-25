using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Money.Web.Models;
using MudBlazor;

namespace Money.Web.Pages;

public partial class CategoryDialog
{
    private bool _isProcessing;

    [Parameter]
    public Category Category {get;set;}

    [CascadingParameter]
    public required MudDialogInstance MudDialog { get; set; }

    [Inject]
    public required ISnackbar SnackbarService { get; set; }

    [Inject]
    public required ILocalStorageService LocalStorage { get; set; }

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
