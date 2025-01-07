using Microsoft.AspNetCore.Components;
using Money.ApiClient;

namespace Money.Web.Pages;

public partial class Categories
{
    private bool _init;

    private Dictionary<int, List<TreeItemData<Category>>> InitialTreeItems { get; } = [];

    [Inject]
    private MoneyClient MoneyClient { get; set; } = null!;

    [Inject]
    private CategoryService CategoryService { get; set; } = null!;

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    [Inject]
    private ISnackbar SnackbarService { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        var categories = await CategoryService.GetAllAsync();

        if (categories.Count == 0)
        {
            return;
        }

        foreach (var operationType in OperationTypes.Values)
        {
            var filteredCategories = categories
                .Where(x => x.OperationType == operationType)
                .ToList();

            InitialTreeItems.Add(operationType.Id, filteredCategories.BuildChildren(null));
        }

        _init = true;
    }

    private async Task Create(OperationTypes.Value operationType, int? parentId)
    {
        var category = new Category
        {
            Name = string.Empty,
            OperationType = operationType,
            ParentId = parentId,
            Color = Random.Shared.NextColor(),
        };

        var createdCategory = await ShowCategoryDialog("Создать", category);

        if (createdCategory == null)
        {
            return;
        }

        AddCategoryToTree(createdCategory, operationType.Id);
        SortTree(category.OperationType.Id);
    }

    private async Task Update(Category category)
    {
        await ShowCategoryDialog("Обновить", category);
        SortTree(category.OperationType.Id);
    }

    private async Task Delete(Category category)
    {
        await ModifyCategory(category, MoneyClient.Category.Delete, true);
    }

    private async Task Restore(Category category)
    {
        await ModifyCategory(category, MoneyClient.Category.Restore, false);
    }

    private async Task<Category?> ShowCategoryDialog(string title, Category category)
    {
        DialogParameters<CategoryDialog> parameters = new()
        {
            { dialog => dialog.Category, category },
        };

        var dialog = await DialogService.ShowAsync<CategoryDialog>(title, parameters);
        return await dialog.GetReturnValueAsync<Category>();
    }

    private async Task ModifyCategory(Category category, Func<int, Task<ApiClientResponse>> action, bool isDeleted)
    {
        if (category.Id == null)
        {
            return;
        }

        var result = await action(category.Id.Value);

        if (result.GetError().ShowMessage(SnackbarService).HasError())
        {
            return;
        }

        category.IsDeleted = isDeleted;
    }

    private void AddCategoryToTree(Category createdCategory, int operationTypeId)
    {
        TreeItemData<Category> addedItem = new()
        {
            Text = createdCategory.Name,
            Value = createdCategory,
        };

        if (createdCategory.ParentId == null)
        {
            InitialTreeItems[operationTypeId].Add(addedItem);
            return;
        }

        var parentItem = SearchParentTreeItem(InitialTreeItems[operationTypeId], createdCategory.ParentId.Value);

        if (parentItem == null)
        {
            return;
        }

        parentItem.Children ??= [];
        parentItem.Children.Add(addedItem);
    }

    private void SortTree(int operationTypeId)
    {
        InitialTreeItems[operationTypeId] = SortChildren(InitialTreeItems[operationTypeId]);
        StateHasChanged();
    }

    private List<TreeItemData<Category>> SortChildren(IEnumerable<TreeItemData<Category>> categories)
    {
        var sortedCategories = categories
            .OrderBy(item => item.Value?.Order == null)
            .ThenBy(item => item.Value?.Order)
            .ThenBy(item => item.Value?.Name)
            .ToList();

        foreach (var category in sortedCategories)
        {
            if (category.Children != null && category.Children.Count != 0)
            {
                category.Children = SortChildren(category.Children);
            }
        }

        return sortedCategories;
    }

    private TreeItemData<Category>? SearchParentTreeItem(List<TreeItemData<Category>>? list, int id)
    {
        if (list == null)
        {
            return null;
        }

        foreach (var item in list)
        {
            if (item.Value?.Id == id)
            {
                return item;
            }

            var result = SearchParentTreeItem(item.Children, id);

            if (result != null)
            {
                return result;
            }
        }

        return null;
    }
}
