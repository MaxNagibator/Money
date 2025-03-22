using Microsoft.AspNetCore.Components;
using Money.ApiClient;

namespace Money.Web.Components.Operations;

public partial class OperationItem(MoneyClient moneyClient, ISnackbar snackbarService)
{
    private OperationDialog _dialog = null!;
    private bool _isStateChanged;

    [Parameter]
    public required Operation Operation { get; set; }

    [Parameter]
    public EventCallback<Operation> OnEdit { get; set; }

    private Category Category => Operation.Category;

    public override Task SetParametersAsync(ParameterView parameters)
    {
        foreach (var parameter in parameters)
        {
            switch (parameter.Name)
            {
                case nameof(Operation):
                    Operation = (Operation)parameter.Value;
                    break;

                case nameof(OnEdit):
                    OnEdit = (EventCallback<Operation>)parameter.Value;
                    break;

                default:
                    throw new ArgumentException($"Unknown parameter: {parameter.Name}");
            }
        }

        return base.SetParametersAsync(ParameterView.Empty);
    }

    protected override bool ShouldRender()
    {
        if (_isStateChanged)
        {
            _isStateChanged = false;
            return true;
        }

        return _dialog.IsOpen;
    }

    private Task Delete()
    {
        return ModifyOperation(moneyClient.Operations.Delete, true);
    }

    private Task Restore()
    {
        return ModifyOperation(moneyClient.Operations.Restore, false);
    }

    private async Task ModifyOperation(Func<int, Task<ApiClientResponse>> action, bool isDeleted)
    {
        if (Operation.Id == null)
        {
            return;
        }

        var result = await action(Operation.Id.Value);

        if (result.GetError().ShowMessage(snackbarService).HasError())
        {
            return;
        }

        Operation.IsDeleted = isDeleted;
        _isStateChanged = true;
    }
}
