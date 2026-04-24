using Money.ApiClient;
using System.Net;

namespace Money.Web.Services;

public class CategoryInferenceService(MoneyClient moneyClient, CategoryService categoryService)
{
    private readonly Dictionary<(string Place, int TypeId), Category?> _cache = [];

    public async Task<Category?> InferAsync(string? place, OperationTypes.Value type, CancellationToken token = default)
    {
        if (string.IsNullOrWhiteSpace(place) || type.Id == 0)
        {
            return null;
        }

        var key = (place, type.Id);

        if (_cache.TryGetValue(key, out var cached))
        {
            return cached;
        }

        var response = await moneyClient.Operations.InferCategoryByPlace(place, type.Id, token: token);

        if (response.Code != HttpStatusCode.OK || response.Content is not { } payload)
        {
            _cache[key] = null;
            return null;
        }

        var categories = await categoryService.GetAllAsync();
        var category = categories.FirstOrDefault(x => x.Id == payload.CategoryId);
        _cache[key] = category;
        return category;
    }

    public void InvalidateCache()
    {
        _cache.Clear();
    }
}
