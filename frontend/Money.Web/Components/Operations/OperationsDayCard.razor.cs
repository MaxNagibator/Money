﻿using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Money.Web.Components.Operations;

// TODO: ShouldRender()
public partial class OperationsDayCard
{
    private OperationDialog _dialog = null!;

    [CascadingParameter]
    public required AppSettings Settings { get; set; }

    [Parameter]
    public required OperationsDay OperationsDay { get; set; }

    [Parameter]
    public required List<FastOperation> FastOperations { get; set; }

    [Parameter]
    public EventCallback<OperationsDay> OnCanDelete { get; set; }

    public override Task SetParametersAsync(ParameterView parameters)
    {
        foreach (var parameter in parameters)
        {
            switch (parameter.Name)
            {
                case nameof(Settings):
                    Settings = (AppSettings)parameter.Value;
                    break;

                case nameof(OperationsDay):
                    OperationsDay = (OperationsDay)parameter.Value;
                    break;

                case nameof(OnCanDelete):
                    OnCanDelete = (EventCallback<OperationsDay>)parameter.Value;
                    break;

                case nameof(FastOperations):
                    FastOperations = (List<FastOperation>)parameter.Value;
                    break;

                default:
                    throw new ArgumentException($"Unknown parameter: {parameter.Name}");
            }
        }

        return base.SetParametersAsync(ParameterView.Empty);
    }

    private RenderFragment RenderOperationButton(OperationTypes.Value type)
    {
        return builder =>
        {
            builder.OpenComponent<MudIconButton>(0);
            builder.AddAttribute(1, "Color", type.Color);
            builder.AddAttribute(2, "Disabled", _dialog.IsOpen);
            builder.AddAttribute(3, "Icon", Icons.Material.Rounded.Add);
            builder.AddAttribute(4, "OnClick", EventCallback.Factory.Create<MouseEventArgs>(this, () => _dialog.ToggleOpen(type)));
            builder.AddAttribute(5, "Size", Size.Small);
            builder.CloseComponent();
        };
    }

    private void OnSubmit(Operation operation)
    {
        if (operation.Date == OperationsDay.Date)
        {
            OperationsDay.Operations.Add(operation);
        }
        else
        {
            OperationsDay.AddAction.Invoke(operation);
        }

        StateHasChanged();
    }

    private async Task OnEdit(Operation operation)
    {
        if (operation.Date != OperationsDay.Date)
        {
            OperationsDay.Operations.Remove(operation);

            if (OperationsDay.Operations.Count == 0)
            {
                await OnCanDelete.InvokeAsync(OperationsDay);
            }

            OperationsDay.AddAction.Invoke(operation);
        }

        StateHasChanged();
    }
}
