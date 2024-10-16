using Money.ApiClient;

namespace Money.Web.Services
{
    public class CategoryService(MoneyClient MoneyClient)
    {
        public async Task<List<Category>?> GetCategories()
        {
            ApiClientResponse<CategoryClient.Category[]> apiCategories = await MoneyClient.Category.Get();

            if (apiCategories.Content == null)
            {
                return null;
            }

            return apiCategories.Content
                .Select(apiCategory => new Category
                {
                    Id = apiCategory.Id,
                    ParentId = apiCategory.ParentId,
                    Name = apiCategory.Name,
                    PaymentTypeId = apiCategory.PaymentTypeId,
                    Color = apiCategory.Color,
                    Order = apiCategory.Order,
                })
                .ToList();
        }
    }
}
