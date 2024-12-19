using Microsoft.AspNetCore.Components;
using Money.Web.Components.FastOperations;

namespace Money.Web.Pages.Operations;

public partial class FastOperations
{
    private List<FastOperation> _operations = [];

    [Inject]
    private FastOperationService FastOperationService { get; set; } = null!;

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        _operations = await FastOperationService.GetAllAsync();
    }

    private async Task Create()
    {
        FastOperation input = new()
        {
            Name = string.Empty,
            Category = Category.Empty,
        };

        FastOperation? created = await ShowDialog("Создать", input);

        if (created == null)
        {
            return;
        }

        _operations!.Insert(0, created);
    }

    private Task<FastOperation?> Update(FastOperation fastOperation)
    {
        return ShowDialog("Обновить", fastOperation);
    }

    private async Task<FastOperation?> ShowDialog(string title, FastOperation fastOperation)
    {
        DialogParameters<FastOperationDialog> parameters = new()
        {
            { dialog => dialog.FastOperation, fastOperation },
        };

        IDialogReference dialog = await DialogService.ShowAsync<FastOperationDialog>(title, parameters);
        return await dialog.GetReturnValueAsync<FastOperation>();
    }
}
