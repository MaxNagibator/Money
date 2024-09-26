using Microsoft.AspNetCore.Components;
using Money.ApiClient;
using Money.Web.Models;
using MudBlazor;
using static MudBlazor.CategoryTypes;

namespace Money.Web.Pages;

public partial class Categories
{
    private bool _init;

    private Dictionary<int, List<TreeItemData<Category>>> InitialTreeItems { get; } = [];

    [Inject]
    private MoneyClient MoneyClient { get; set; } = default!;

    [Inject]
    private IDialogService DialogService { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        ApiClientResponse<CategoryClient.Category[]> apiCategories = await MoneyClient.Category.Get();

        if (apiCategories.Content == null)
        {
            return;
        }

        List<Category> categories = apiCategories.Content.Select(apiCategory => new Category
        {
            Id = apiCategory.Id,
            ParentId = apiCategory.ParentId,
            Name = apiCategory.Name,
            PaymentTypeId = apiCategory.PaymentTypeId
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
        var defaultColor = "#9b9b9bff";
        DialogParameters<CategoryDialog> parameters = new()
        {
            { dialog => dialog.Category, new Category { ParentId = parentId, Id = null, PaymentTypeId = paymentType.Id, Color = defaultColor } }
        };

        var dialog = DialogService.Show<CategoryDialog>("Создать", parameters);
        var result = await dialog.Result;
        if (result.Canceled == false)
        {
            var createdCategory = result.Data as Category;
            var asditem = new TreeItemData<Category>
            {
                Text = createdCategory.Name,
                Value = createdCategory,
            };
            if (createdCategory.ParentId == null)
            {
                InitialTreeItems[paymentType.Id].Add(asditem);
            }
            else
            {
                var item = SearchParentTreeItem(InitialTreeItems[paymentType.Id], createdCategory.ParentId.Value);
                if (item.Children == null)
                {
                    item.Children = new List<TreeItemData<Category>>();
                }
                item.Children.Add(asditem);
            }
        }
    }

    private async Task Delete(Category category)
    {
        var result = await MoneyClient.Category.Delete(category.Id.Value);
        if (result.IsSuccessStatusCode)
        {
            var item = SearchParentTreeItem(InitialTreeItems[category.PaymentTypeId], category.Id.Value);
            if (category.ParentId == null)
            {
                InitialTreeItems[category.PaymentTypeId].Remove(item);
            }
            else
            {
                // todo не удаляется элемент не в корне
                var parentItem = SearchParentTreeItem(InitialTreeItems[category.PaymentTypeId], category.ParentId.Value);
                parentItem.Children.Remove(item);
            }
        }
    }

    private TreeItemData<Category>? SearchParentTreeItem(List<TreeItemData<Category>> list, int id)
    {
        foreach (var item in list)
        {
            if (item.Value.Id == id)
            {
                return item;
            }
            var result = SearchParentTreeItem(item.Children, id);
            if (result != null)
            {
                return item;
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
                Children = BuildChildren(categories, child.Id)
            })
            .ToList();
    }
}
