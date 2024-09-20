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
        TestCategory firstCategory = user.WithCategory();
        user.WithCategory();
        user.WithCategory();
        dbClient.Save();

        CategoryClient apiClient = new(Integration.GetHttpClient(), Console.WriteLine);
        apiClient.SetUser(user);
        CategoryClient.GetCategoriesModel? result = (await apiClient.Get(1)).IsSuccess().Content;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Categories, Is.Not.Null);
        Assert.That(result.Categories, Is.Not.Empty);
        Assert.That(result.Categories.Count, Is.EqualTo(3));
        CategoryClient.GetCategoriesModel.CategoryValue? apiCategory = result.Categories.FirstOrDefault(x => x.Id == firstCategory.Id);
        Assert.That(apiCategory, Is.Not.Null);
        Assert.That(apiCategory.Name, Is.EqualTo(firstCategory.Name));
    }
}
