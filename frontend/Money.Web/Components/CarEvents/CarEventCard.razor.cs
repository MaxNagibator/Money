using Microsoft.AspNetCore.Components;
using Money.ApiClient;

namespace Money.Web.Components.CarEvents;

public partial class CarEventCard : ComponentBase
{
    [Parameter]
    public CarEvent Model { get; set; } = null!;

    [Parameter]
    public EventCallback<CarEvent> OnUpdate { get; set; }

    [Inject]
    private MoneyClient MoneyClient { get; set; } = null!;

    [Inject]
    private ISnackbar SnackbarService { get; set; } = null!;

    private string ClassName => Model.IsDeleted ? "deleted-card" : "card";

    private Task Update(CarEvent model)
    {
        return OnUpdate.InvokeAsync(model);
    }

    private Task Delete(CarEvent model)
    {
        return Modify(model, MoneyClient.CarEvents.Delete, true);
    }

    private Task Restore(CarEvent model)
    {
        return Modify(model, MoneyClient.CarEvents.Restore, false);
    }

    private async Task Modify(CarEvent model, Func<int, Task<ApiClientResponse>> action, bool isDeleted)
    {
        if (model.Id == null)
        {
            return;
        }

        var response = await action(model.Id.Value);

        if (response.GetError().ShowMessage(SnackbarService).HasError())
        {
            return;
        }

        model.IsDeleted = isDeleted;
    }
}
