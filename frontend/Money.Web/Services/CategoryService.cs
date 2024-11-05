using Money.Api.Constracts.Categories;
using Money.ApiClient;

namespace Money.Web.Services;

public class CategoryService(MoneyClient moneyClient)
{
    public async Task<List<Category>?> GetCategories()
    {
        IEnumerable<CategoryDTO> apiCategories = await moneyClient.Categories.GetListAsync();

        return apiCategories?.Select(apiCategory => new Category
        {
            Id = apiCategory.Id,
            ParentId = apiCategory.ParentId,
            Name = apiCategory.Name,
            Color = apiCategory.Color,
            Order = apiCategory.Order,
            OperationType = OperationTypes.Values.First(x => x.Id == apiCategory.OperationTypeId),
        })
            .ToList();
    }
}
