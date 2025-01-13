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

    private async Task Create(OperationTypes.Value? type = null)
    {
        var input = new FastOperation
        {
            Name = string.Empty,
            Category = Category.Empty,
        };

        var created = await ShowDialog("Создать", input, type);

        if (created == null)
        {
            return;
        }

        _operations.Insert(0, created);
    }

    private Task<FastOperation?> Update(FastOperation model)
    {
        return ShowDialog("Обновить", model);
    }

    private async Task<FastOperation?> ShowDialog(string title, FastOperation model, OperationTypes.Value? type = null)
    {
        DialogParameters<FastOperationDialog> parameters = new()
        {
            { dialog => dialog.Model, model },
            { dialog => dialog.RequiredType, type },
        };

        var dialog = await DialogService.ShowAsync<FastOperationDialog>(title, parameters);
        return await dialog.GetReturnValueAsync<FastOperation>();
    }
}
