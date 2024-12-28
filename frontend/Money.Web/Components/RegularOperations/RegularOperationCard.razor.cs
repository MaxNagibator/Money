using Microsoft.AspNetCore.Components;
using Money.ApiClient;

namespace Money.Web.Components.RegularOperations;

public partial class RegularOperationCard : ComponentBase
{
    [Parameter]
    public RegularOperation Operation { get; set; } = null!;

    [Parameter]
    public EventCallback<RegularOperation> OnUpdate { get; set; }

    [Inject]
    private MoneyClient MoneyClient { get; set; } = null!;

    [Inject]
    private ISnackbar SnackbarService { get; set; } = null!;

    // TODO: Подумать как более грамотно сделать
    private string ClassName => Operation.IsDeleted
        ? "deleted-operation-card"
        : Operation.Category.OperationType.Id == 1
            ? "expense-operation-card"
            : "income-operation-card";

    private Task Update(RegularOperation RegularOperation)
    {
        return OnUpdate.InvokeAsync(RegularOperation);
    }

    private Task Delete(RegularOperation RegularOperation)
    {
        return Modify(RegularOperation, MoneyClient.RegularOperation.Delete, true);
    }

    private Task Restore(RegularOperation RegularOperation)
    {
        return Modify(RegularOperation, MoneyClient.RegularOperation.Restore, false);
    }

    private async Task Modify(RegularOperation RegularOperation, Func<int, Task<ApiClientResponse>> action, bool isDeleted)
    {
        if (RegularOperation.Id == null)
        {
            return;
        }

        ApiClientResponse result = await action(RegularOperation.Id.Value);

        if (result.GetError().ShowMessage(SnackbarService).HasError())
        {
            return;
        }

        RegularOperation.IsDeleted = isDeleted;
    }
}
