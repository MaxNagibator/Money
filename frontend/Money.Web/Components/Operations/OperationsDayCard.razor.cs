using Microsoft.AspNetCore.Components;

namespace Money.Web.Components.Operations;

public partial class OperationsDayCard
{
    private OperationDialog _dialog = null!;

    [CascadingParameter]
    public required AppSettings Settings { get; set; }

    [Parameter]
    public required OperationsDay OperationsDay { get; set; }

    [Parameter]
    public required OperationTypes.Value[] OperationTypes { get; set; }

    [Parameter]
    public EventCallback<Operation> OnAddOperation { get; set; }

    [Parameter]
    public EventCallback<Operation> OnRestore { get; set; }

    [Parameter]
    public EventCallback<Operation> OnDelete { get; set; }

    [Parameter]
    public EventCallback<OperationsDay> OnCanDelete { get; set; }

    public override Task SetParametersAsync(ParameterView parameters)
    {
        foreach (ParameterValue parameter in parameters)
        {
            switch (parameter.Name)
            {
                case nameof(Settings):
                    Settings = (AppSettings)parameter.Value;
                    break;

                case nameof(OperationsDay):
                    OperationsDay = (OperationsDay)parameter.Value;
                    break;

                case nameof(OperationTypes):
                    OperationTypes = (OperationTypes.Value[])parameter.Value;
                    break;

                case nameof(OnAddOperation):
                    OnAddOperation = (EventCallback<Operation>)parameter.Value;
                    break;

                case nameof(OnRestore):
                    OnRestore = (EventCallback<Operation>)parameter.Value;
                    break;

                case nameof(OnDelete):
                    OnDelete = (EventCallback<Operation>)parameter.Value;
                    break;

                case nameof(OnCanDelete):
                    OnCanDelete = (EventCallback<OperationsDay>)parameter.Value;
                    break;

                default:
                    throw new ArgumentException($"Unknown parameter: {parameter.Name}");
            }
        }

        return base.SetParametersAsync(ParameterView.Empty);
    }

    private async Task OnSubmit(Operation operation)
    {
        if (operation.Date == OperationsDay.Date)
        {
            OperationsDay.Operations.Add(operation);
        }
        else
        {
            await OnAddOperation.InvokeAsync(operation);
        }
    }

    private async Task OnEdit(Operation operation)
    {
        if (operation.Date == OperationsDay.Date)
        {
            StateHasChanged();
            return;
        }

        OperationsDay.Operations.Remove(operation);

        if (OperationsDay.Operations.Count == 0)
        {
            await OnCanDelete.InvokeAsync(OperationsDay);
        }

        await OnAddOperation.InvokeAsync(operation);
        StateHasChanged();
    }
}
