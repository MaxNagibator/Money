using Microsoft.AspNetCore.Components;
using Money.ApiClient;
using Money.Web.Components.FastOperations;

namespace Money.Web.Pages.Operations;

public partial class FastOperations
{
    private List<Category>? _categories;

    // TODO: Возможно не лучший шаг с двумя списками
    private List<FastOperation>? _operations;
    private List<FastOperation>? _deletedOperations;

    [Inject]
    private MoneyClient MoneyClient { get; set; } = null!;

    [Inject]
    private CategoryService CategoryService { get; set; } = null!;

    [Inject]
    private FastOperationService FastOperationService { get; set; } = null!;

    [Inject]
    private ISnackbar SnackbarService { get; set; } = null!;

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        _categories = await CategoryService.GetCategories();
        _operations = await FastOperationService.GetFastOperations(_categories!);
        _deletedOperations = [];
    }

    private async Task Create()
    {
        FastOperation input = new()
        {
            Name = string.Empty,
            Category = Category.Empty,
        };

        FastOperation? created = await ShowDialog("Создать", input);

        if (created == null)
        {
            return;
        }

        _operations!.Insert(0, created);
    }

    private Task<FastOperation?> Update(FastOperation fastOperation)
    {
        return ShowDialog("Обновить", fastOperation);
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

        ApiClientResponse result = await action(fastOperation.Id.Value);

        if (result.GetError().ShowMessage(SnackbarService).HasError())
        {
            return;
        }

        fastOperation.IsDeleted = isDeleted;

        if (isDeleted)
        {
            _operations!.Remove(fastOperation);
            _deletedOperations!.Insert(0, fastOperation);
        }
        else
        {
            _deletedOperations!.Remove(fastOperation);
            _operations!.Insert(0, fastOperation);
        }
    }

    private async Task<FastOperation?> ShowDialog(string title, FastOperation fastOperation)
    {
        DialogParameters<FastOperationDialog> parameters = new()
        {
            { dialog => dialog.FastOperation, fastOperation },
            // TODO: Подумать над инжектом CategoryService в FastOperationDialog
            { dialog => dialog.Categories, _categories },
        };

        IDialogReference dialog = await DialogService.ShowAsync<FastOperationDialog>(title, parameters);
        return await dialog.GetReturnValueAsync<FastOperation>();
    }
}
