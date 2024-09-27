using Microsoft.AspNetCore.Components;
using Money.ApiClient;

namespace Money.Web.Pages;

public partial class Categories
{
    private bool _init;

    private Dictionary<int, List<TreeItemData<Category>>> InitialTreeItems { get; } = [];

    [Inject]
    private MoneyClient MoneyClient { get; set; } = default!;

    [Inject]
    private IDialogService DialogService { get; set; } = default!;

    [Inject]
    private ISnackbar SnackbarService { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        ApiClientResponse<CategoryClient.Category[]> apiCategories = await MoneyClient.Category.Get();

        if (apiCategories.Content == null)
        {
            return;
        }

        List<Category> categories = apiCategories.Content
            .Select(apiCategory => new Category
            {
                Id = apiCategory.Id,
                ParentId = apiCategory.ParentId,
                Name = apiCategory.Name,
                PaymentTypeId = apiCategory.PaymentTypeId,
                Color = apiCategory.Color,
                Order = apiCategory.Order ?? 0,
            })
            .ToList();

        foreach (PaymentTypes.Value paymentType in PaymentTypes.Values)
        {
            InitialTreeItems.Add(paymentType.Id, BuildChildren(categories.Where(x => x.PaymentTypeId == paymentType.Id).ToList(), null));
        }

        _init = true;
    }

    private async Task Create(PaymentTypes.Value paymentType, int? parentId)
    {
        string defaultColor = "#9b9b9bff";

        DialogParameters<CategoryDialog> parameters = new()
        {
            { dialog => dialog.Category, new Category { ParentId = parentId, Id = null, PaymentTypeId = paymentType.Id, Color = defaultColor } },
        };

        IDialogReference dialog = await DialogService.ShowAsync<CategoryDialog>("Создать", parameters);
        Category? createdCategory = await dialog.GetReturnValueAsync<Category>();

        if (createdCategory != null)
        {
            TreeItemData<Category> addedItem = new()
            {
                Text = createdCategory.Name,
                Value = createdCategory,
            };

            if (createdCategory.ParentId == null)
            {
                InitialTreeItems[paymentType.Id].Add(addedItem);
            }
            else
            {
                TreeItemData<Category>? item = SearchParentTreeItem(InitialTreeItems[paymentType.Id], createdCategory.ParentId.Value);

                item.Children ??= [];

                item.Children.Add(addedItem);
            }
        }
    }

    private async Task Delete(Category category)
    {
        ApiClientResponse result = await MoneyClient.Category.Delete(category.Id.Value);

        if (result.IsSuccessStatusCode)
        {
            TreeItemData<Category>? item = SearchParentTreeItem(InitialTreeItems[category.PaymentTypeId], category.Id.Value);

            if (category.ParentId == null)
            {
                InitialTreeItems[category.PaymentTypeId].Remove(item);
            }
            else
            {
                TreeItemData<Category>? parentItem = SearchParentTreeItem(InitialTreeItems[category.PaymentTypeId], category.ParentId.Value);
                parentItem.Children.Remove(item);
            }
        }
        else
        {
            var message = result.StringContent; // todo обработать бизнес ошибки
            SnackbarService.Add("Ошибка: " + message, Severity.Error);
        }
    }

    private TreeItemData<Category>? SearchParentTreeItem(List<TreeItemData<Category>> list, int id)
    {
        if (list == null)
        {
            return null;
        }
        foreach (TreeItemData<Category> item in list)
        {
            if (item.Value.Id == id)
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
            .ToList();
    }
}
