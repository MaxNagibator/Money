using Money.ApiClient;

namespace Money.Web.Services;

public class FastOperationService(MoneyClient moneyClient)
{
    public async Task<List<FastOperation>?> GetFastOperations(List<Category> categories)
    {
        ApiClientResponse<FastOperationClient.FastOperation[]> apiCategories = await moneyClient.FastOperation.Get();
        var cats = categories.ToDictionary(x => x.Id!.Value, x => x);
        return apiCategories.Content?.Select(apiCategory => new FastOperation
        {
            Id = apiCategory.Id,
            Sum = apiCategory.Sum,
            Name = apiCategory.Name,
            Comment = apiCategory.Comment,
            Order = apiCategory.Order,
            Category = cats[apiCategory.CategoryId], // todo а если удалённая категория? 
            Place = apiCategory.Place,
        })
        .ToList();
    }
}
