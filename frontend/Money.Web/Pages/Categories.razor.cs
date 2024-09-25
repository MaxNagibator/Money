using Microsoft.AspNetCore.Components;
using Money.ApiClient;

namespace Money.Web.Pages;

public partial class Categories
{
    private bool _init;

    private Dictionary<int, List<TreeItemData<Category>>> InitialTreeItems { get; } = [];

    [Inject]
    private HttpClient HttpClient { get; set; } = default!;

    [Inject]
    private IDialogService DialogService { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        MoneyClient apiClient = new(HttpClient, Console.WriteLine);
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
            })
            .ToList();

        foreach (PaymentTypes.Value paymentType in PaymentTypes.Values)
        {
            InitialTreeItems.Add(paymentType.Id, BuildChildren(categories.Where(x => x.PaymentTypeId == paymentType.Id).ToList(), null));
        }

        _init = true;
    }

    private void Create(PaymentTypes.Value paymentType)
    {
        DialogParameters<CategoryDialog> parameters = new()
        {
            { dialog => dialog.Category, new Category { ParentId = null, Id = null, PaymentTypeId = paymentType.Id } }
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
