using Money.ApiClient;
using MudBlazor;

namespace Money.Web.Pages;

public partial class Categories
{
    private List<TreeItemData<Category>> InitialTreeItems { get; } = [];

    protected override async Task OnInitializedAsync()
    {
        HttpClient client = new()
        {
            BaseAddress = new Uri("https://localhost:7124/")
        };

        MoneyClient apiClient = new(client, Console.WriteLine);
        apiClient.SetUser("bob217@mail.ru", "123123123");

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

        InitialTreeItems.AddRange(BuildChildren(categories, null));
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

public class Category
{
    /// <summary>
    ///     Идентификатор категории.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    ///     Название категории.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    ///     Идентификатор родительской категории (если есть).
    /// </summary>
    public int? ParentId { get; set; }

    /// <summary>
    ///     Порядок отображения категории.
    /// </summary>
    public int? Order { get; set; }

    /// <summary>
    ///     Цвет категории.
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    ///     Идентификатор типа платежа.
    /// </summary>
    public required int PaymentTypeId { get; set; }

    public override string ToString()
    {
        return Name;
    }
}
