using Money.Api.Tests.ApiClient;
using Money.Api.Tests.TestTools;
using Money.Business.Enums;

namespace Money.Api.Tests;

internal class CategoryClient(HttpClient client, Action<string> log) : ApiClientExecutor(client, log)
{
    protected override string ApiPrefix => "";

    public async Task<ApiClientResponse<GetCategoriesModel>> Get(int? type = null)
    {
        string paramUri = type == null ? "" : "?type=" + type;
        return await GetAsync<GetCategoriesModel>("/Categories" + paramUri);
    }

    public class GetCategoriesModel
    {
        public CategoryValue[] Categories { get; set; }

        public class CategoryValue
        {
            public int Id { get; set; }

            public required string Name { get; set; }

            public int? ParentId { get; set; }

            public int? Order { get; set; }

            public string? Color { get; set; }

            public PaymentTypes PaymentType { get; set; }
        }
    }
}
