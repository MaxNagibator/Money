﻿using Microsoft.AspNetCore.Components;
using Money.ApiClient;
using Money.Web.Components.FastOperations;

namespace Money.Web.Pages.Operations;

public partial class FastOperations
{
    private List<Category>? _categories;

    private List<FastOperation>? _operations;

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
        _categories = await CategoryService.GetAllAsync();
        _operations = await FastOperationService.GetAllAsync();
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
    }

    private async Task<FastOperation?> ShowDialog(string title, FastOperation fastOperation)
    {
        DialogParameters<FastOperationDialog> parameters = new()
        {
            { dialog => dialog.FastOperation, fastOperation },
        };

        IDialogReference dialog = await DialogService.ShowAsync<FastOperationDialog>(title, parameters);
        return await dialog.GetReturnValueAsync<FastOperation>();
    }
}
