using Money.Api.Tests.ApiClient;

namespace Money.Api.Tests;

public class CategoryTests
{
    [Test]
    public async Task GetModifiedTest()
    {
        CategoryClient apiClient = new(Integration.GetHttpClient(), Console.WriteLine);
        CategoryClient.GetCategoriesModel? result = (await apiClient.Get(1)).IsSuccess().Content;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Categories, Is.Not.Null);
        Assert.That(result.Categories, Is.Not.Empty);
    }
}
