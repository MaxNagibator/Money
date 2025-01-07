using Microsoft.AspNetCore.Components;

namespace Money.Web.Components.Operations;

public partial class OperationItem
{
    private OperationDialog _dialog = null!;

    [Parameter]
    public required Operation Operation { get; set; }

    [Parameter]
    public EventCallback<Operation> OnEdit { get; set; }

    [Parameter]
    public EventCallback<Operation> OnDelete { get; set; }

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

                case nameof(OnDelete):
                    OnDelete = (EventCallback<Operation>)parameter.Value;
                    break;

                default:
                    throw new ArgumentException($"Unknown parameter: {parameter.Name}");
            }
        }

        return base.SetParametersAsync(ParameterView.Empty);
    }

    protected override bool ShouldRender()
    {
        return _dialog.IsOpen;
    }
}
