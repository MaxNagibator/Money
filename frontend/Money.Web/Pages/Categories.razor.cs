using Microsoft.AspNetCore.Components;
using Money.ApiClient;
using Money.Web.Components;

namespace Money.Web.Pages;

public partial class Categories
{
    private bool _init;

    private Dictionary<int, List<TreeItemData<Category>>> InitialTreeItems { get; } = [];

    [Inject]
    private MoneyClient MoneyClient { get; set; } = default!;

    [Inject]
    private CategoryService CategoryService { get; set; } = default!;

    [Inject]
    private IDialogService DialogService { get; set; } = default!;

    [Inject]
    private ISnackbar SnackbarService { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        List<Category>? categories = await CategoryService.GetCategories();

        if (categories == null)
        {
            return;
        }

        foreach (PaymentTypes.Value paymentType in PaymentTypes.Values)
        {
            InitialTreeItems.Add(paymentType.Id, BuildChildren(categories.Where(x => x.PaymentTypeId == paymentType.Id).ToList(), null));
        }

        _init = true;
    }

    private async Task Create(PaymentTypes.Value paymentType, int? parentId)
    {
        Category category = new()
        {
            PaymentType = paymentType,
            PaymentTypeId = paymentType.Id,
            ParentId = parentId,
            Color = "#9b9b9bff",
        };

        Category? createdCategory = await ShowCategoryDialog("Создать", category);

        if (createdCategory == null)
        {
            return;
        }

        AddCategoryToTree(createdCategory, paymentType.Id);
        SortTree(category.PaymentTypeId);
    }

    private async Task Update(Category category)
    {
        await ShowCategoryDialog("Обновить", category);
        SortTree(category.PaymentTypeId);
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

        IDialogReference dialog = await DialogService.ShowAsync<CategoryDialog>(title, parameters);
        return await dialog.GetReturnValueAsync<Category>();
    }

    private async Task ModifyCategory(Category category, Func<int, Task<ApiClientResponse>> action, bool isDeleted)
    {
        if (category.Id == null)
        {
            return;
        }

        ApiClientResponse result = await action(category.Id.Value);

        if (result.GetError().ShowMessage(SnackbarService).HasError())
        {
            return;
        }

        category.IsDeleted = isDeleted;
    }

    private void AddCategoryToTree(Category createdCategory, int paymentTypeId)
    {
        TreeItemData<Category> addedItem = new()
        {
            Text = createdCategory.Name,
            Value = createdCategory,
        };

        if (createdCategory.ParentId == null)
        {
            InitialTreeItems[paymentTypeId].Add(addedItem);
            return;
        }

        TreeItemData<Category>? parentItem = SearchParentTreeItem(InitialTreeItems[paymentTypeId], createdCategory.ParentId.Value);

        if (parentItem == null)
        {
            return;
        }

        parentItem.Children ??= [];
        parentItem.Children.Add(addedItem);
    }

    private void SortTree(int paymentTypeId)
    {
        InitialTreeItems[paymentTypeId] = SortChildren(InitialTreeItems[paymentTypeId]);
        StateHasChanged();
    }

    private List<TreeItemData<Category>> SortChildren(IEnumerable<TreeItemData<Category>> categories)
    {
        List<TreeItemData<Category>> sortedCategories = categories
            .OrderBy(item => item.Value?.Order == null)
            .ThenBy(item => item.Value?.Order)
            .ThenBy(item => item.Value?.Name)
            .ToList();

        foreach (TreeItemData<Category> category in sortedCategories)
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

        foreach (TreeItemData<Category> item in list)
        {
            if (item.Value?.Id == id)
            {
                return item;
            }

            TreeItemData<Category>? result = SearchParentTreeItem(item.Children, id);

            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    private List<TreeItemData<Category>> BuildChildren(List<Category> categories, int? parentId)
    {
        return categories.Where(category => category.ParentId == parentId)
            .Select(child => new TreeItemData<Category>
            {
                Text = child.Name,
                Value = child,
                Children = BuildChildren(categories, child.Id),
            })
            .OrderBy(item => item.Value?.Order == null)
            .ThenBy(item => item.Value?.Order)
            .ThenBy(item => item.Value?.Name)
            .ToList();
    }
}
