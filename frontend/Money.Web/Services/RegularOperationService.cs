using Money.ApiClient;

namespace Money.Web.Services;

public class RegularOperationService(MoneyClient moneyClient, CategoryService categoryService)
{
    // TODO: Подумать над вынесением ToDictionary() в CategoryService.
    public async Task<List<RegularOperation>> GetAllAsync()
    {
        ApiClientResponse<RegularOperationClient.RegularOperation[]> opertions = await moneyClient.RegularOperation.Get();
        Dictionary<int, Category> categories = (await categoryService.GetAllAsync()).ToDictionary(x => x.Id!.Value, x => x);

        return opertions.Content?
                   .Select(apiCategory => new RegularOperation
                   {
                       Id = apiCategory.Id,
                       Sum = apiCategory.Sum,
                       Name = apiCategory.Name,
                       Comment = apiCategory.Comment,
                       Category = categories.GetValueOrDefault(apiCategory.CategoryId, Category.Empty),
                       Place = apiCategory.Place,
                       DateFrom = apiCategory.DateFrom,
                       DateTo = apiCategory.DateTo,
                       TimeType = RegularOperationTimeTypes.Values[apiCategory.TimeTypeId],
                       TimeValue = apiCategory.TimeValue,
                       RunTime = apiCategory.RunTime,
                   })
                   .OrderBy(x => x.RunTime == null)
                   .ThenBy(x => x.RunTime)
                   .ToList()
               ?? [];
    }
}
