using Money.ApiClient;

namespace Money.Web.Services;

public class FastOperationService(MoneyClient moneyClient, CategoryService categoryService)
{
    // TODO: Подумать над вынесением ToDictionary() в CategoryService.
    public async Task<List<FastOperation>> GetAllAsync()
    {
        var response = await moneyClient.FastOperation.Get();
        var categories = (await categoryService.GetAllAsync()).ToDictionary(x => x.Id!.Value, x => x);

        return response.Content?
                   .Select(apiCategory => new FastOperation
                   {
                       Id = apiCategory.Id,
                       Sum = apiCategory.Sum,
                       Name = apiCategory.Name,
                       Comment = apiCategory.Comment,
                       Order = apiCategory.Order,
                       Category = categories.GetValueOrDefault(apiCategory.CategoryId, Category.Empty),
                       Place = apiCategory.Place,
                   })
                   .OrderBy(x => x.Order == null)
                   .ThenBy(x => x.Order)
                   .ThenBy(x => x.Category.Name)
                   .ThenBy(x => x.Name)
                   .ToList()
               ?? [];
    }
}
