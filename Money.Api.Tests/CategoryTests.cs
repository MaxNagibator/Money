using Money.Api.Tests.ApiClient;
using Money.Api.Tests.TestTools;
using Money.Api.Tests.TestTools.Entities;

namespace Money.Api.Tests;

public class CategoryTests
{
    [Test]
    public async Task GetModifiedTest()
    {
        DatabaseClient dbClient = Integration.GetDatabaseClient();
        TestUser user = dbClient.WithUser();

        TestCategory[] categories =
        [
            user.WithCategory(),
            user.WithCategory(),
            user.WithCategory()
        ];

        dbClient.Save();

        CategoryClient apiClient = new(Integration.GetHttpClient(), Console.WriteLine);
        apiClient.SetUser(user);

        GetCategoriesModel? result = (await apiClient.Get(1)).IsSuccess().Content;
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Categories, Is.Not.Null);
        Assert.That(result.Categories, Is.Not.Empty);
        Assert.That(result.Categories.Count, Is.EqualTo(3));

        TestCategory[] testCategories = categories.ExceptBy(result.Categories.Select(x => x.Id), category => category.Id).ToArray();
        Assert.That(testCategories, Is.Not.Null);
        Assert.That(testCategories, Is.Empty);
    }
}
