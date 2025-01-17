using Microsoft.AspNetCore.Components;
using Money.Web.Components.RegularOperations;

namespace Money.Web.Pages.Operations;

public partial class RegularOperations
{
    private List<RegularOperation> _operations = [];

    [Inject]
    private RegularOperationService RegularOperationService { get; set; } = null!;

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        _operations = await RegularOperationService.GetAllAsync();
    }

    private async Task Create(OperationTypes.Value? type = null)
    {
        var input = new RegularOperation
        {
            Name = string.Empty,
            Category = Category.Empty,
            DateFrom = DateTime.Now,
            TimeType = RegularOperationTimeTypes.Values.Skip(2).First().Value,
            TimeValue = 1,
        };

        var created = await ShowDialog("Создать", input, type);

        if (created == null)
        {
            return;
        }

        _operations.Insert(0, created);
    }

    private Task<RegularOperation?> Update(RegularOperation model)
    {
        return ShowDialog("Обновить", model);
    }

    private async Task<RegularOperation?> ShowDialog(string title, RegularOperation model, OperationTypes.Value? type = null)
    {
        DialogParameters<RegularOperationDialog> parameters = new()
        {
            { dialog => dialog.Model, model },
            { dialog => dialog.RequiredType, type },
        };

        var dialog = await DialogService.ShowAsync<RegularOperationDialog>(title, parameters);
        return await dialog.GetReturnValueAsync<RegularOperation>();
    }
}
