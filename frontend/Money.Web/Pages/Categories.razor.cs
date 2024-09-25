using Microsoft.AspNetCore.Components;
using Money.ApiClient;
using Money.Web.Models;
using Money.Web.Services;
using MudBlazor;

namespace Money.Web.Pages;

public partial class Categories
{
    private bool _init;

    private Dictionary<int, List<TreeItemData<Category>>> InitialTreeItems { get; } = [];

    [Inject]
    private IHttpClientFactory HttpClient { get; set; }

    [Inject]
    private IDialogService DialogService { get; set; }

    protected override async Task OnInitializedAsync()
    {
        MoneyClient apiClient = new(HttpClient.CreateClient("api"), Console.WriteLine);
        ApiClientResponse<CategoryClient.Category[]> apiCategories = await apiClient.Category.Get();

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
        }).ToList();

        foreach (var paymentType in PaymentTypes.Values)
        {
            InitialTreeItems.Add(paymentType.Id, BuildChildren(categories.Where(x => x.PaymentTypeId == paymentType.Id).ToList(), null));
        }
        _init = true;
    }

    private void Create()
    {
        DialogParameters<CategoryDialog> parameters = new()
        {
            { dialog => dialog.Category, new Category { ParentId = null, Id = null, PaymentTypeId = 1 } },
        };
        DialogService.Show<CategoryDialog>("Создать", parameters);
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

