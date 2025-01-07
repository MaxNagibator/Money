using Microsoft.AspNetCore.Components;
using Money.ApiClient;

namespace Money.Web.Components.FastOperations;

public partial class FastOperationCard : ComponentBase
{
    [Parameter]
    public FastOperation Operation { get; set; } = null!;

    [Parameter]
    public EventCallback<FastOperation> OnUpdate { get; set; }

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

    private Task Update(FastOperation fastOperation)
    {
        return OnUpdate.InvokeAsync(fastOperation);
    }

    private Task Delete(FastOperation fastOperation)
    {
        return Modify(fastOperation, MoneyClient.FastOperation.Delete, true);
    }

    private Task Restore(FastOperation fastOperation)
    {
        return Modify(fastOperation, MoneyClient.FastOperation.Restore, false);
    }

    private async Task Modify(FastOperation fastOperation, Func<int, Task<ApiClientResponse>> action, bool isDeleted)
    {
        if (fastOperation.Id == null)
        {
            return;
        }

        var result = await action(fastOperation.Id.Value);

        if (result.GetError().ShowMessage(SnackbarService).HasError())
        {
            return;
        }

        fastOperation.IsDeleted = isDeleted;
    }
}
