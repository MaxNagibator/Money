using Microsoft.AspNetCore.Components;
using Money.ApiClient;
using Money.Web.Components.FastOperations;

namespace Money.Web.Pages.Operations;

public partial class FastOperations
{
    // TODO: Добавить диалог для быстрых операций как в категориях.
    private FastOperationDialog _dialog = null!;

    [Inject]
    private MoneyClient MoneyClient { get; set; } = null!;

    [Inject]
    private CategoryService CategoryService { get; set; } = null!;

    [Inject]
    private FastOperationService FastOperationService { get; set; } = null!;

    [Inject]
    private ISnackbar SnackbarService { get; set; } = null!;

    private List<Category>? Categories { get; set; }
    private List<FastOperation>? Operations { get; set; }

    protected override async Task OnInitializedAsync()
    {
        Categories = await CategoryService.GetCategories();
        Operations = await FastOperationService.GetFastOperations(Categories!);
    }

    private void AddNewOperation(FastOperation operation)
    {
        Operations!.Insert(0, operation);
        StateHasChanged();
    }

    private void OnEdit(FastOperation operation)
    {
        StateHasChanged();
    }

    private async Task Delete(FastOperation fastOperation)
    {
        await ModifiyEntity(fastOperation, MoneyClient.FastOperation.Delete, true);
    }

    private async Task Restore(FastOperation fastOperation)
    {
        await ModifiyEntity(fastOperation, MoneyClient.FastOperation.Restore, false);
    }

    private async Task ModifiyEntity(FastOperation fastOperation, Func<int, Task<ApiClientResponse>> action, bool isDeleted)
    {
        if (fastOperation.Id == null)
        {
            return;
        }

        ApiClientResponse result = await action(fastOperation.Id.Value);

        if (result.GetError().ShowMessage(SnackbarService).HasError())
        {
            return;
        }

        fastOperation.IsDeleted = isDeleted;
    }
}
