using Microsoft.AspNetCore.Components;
using Money.ApiClient;

namespace Money.Web.Components.Debts;

public partial class DebtCard : ComponentBase
{
    [Parameter]
    public Debt Model { get; set; } = null!;

    [Parameter]
    public EventCallback<Debt> OnUpdate { get; set; }

    [Inject]
    private MoneyClient MoneyClient { get; set; } = null!;

    [Inject]
    private ISnackbar SnackbarService { get; set; } = null!;

    // TODO: Подумать как более грамотно сделать
    private string ClassName => Model.IsDeleted
        ? "deleted-operation-card"
        : string.Empty;

    private Task Update(Debt entity)
    {
        return OnUpdate.InvokeAsync(entity);
    }

    private Task Delete(Debt entity)
    {
        return Modify(entity, MoneyClient.Debt.Delete, true);
    }

    private Task Restore(Debt entity)
    {
        return Modify(entity, MoneyClient.Debt.Restore, false);
    }

    private async Task Modify(Debt entity, Func<int, Task<ApiClientResponse>> action, bool isDeleted)
    {
        if (entity.Id == null)
        {
            return;
        }

        var result = await action(entity.Id.Value);

        if (result.GetError().ShowMessage(SnackbarService).HasError())
        {
            return;
        }

        entity.IsDeleted = isDeleted;
    }
}
