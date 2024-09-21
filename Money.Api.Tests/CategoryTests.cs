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

        CategoryClient.Category[]? apiCategories = (await apiClient.Get(1)).IsSuccess().Content;
        Assert.That(apiCategories, Is.Not.Null);
        Assert.That(apiCategories.Count, Is.EqualTo(3));

        TestCategory[] testCategories = categories.ExceptBy(apiCategories.Select(x => x.Id), category => category.Id).ToArray();
        Assert.That(testCategories, Is.Not.Null);
        Assert.That(testCategories, Is.Empty);
    }

    [Test]
    public async Task GetByIdTest()
    {
        DatabaseClient dbClient = Integration.GetDatabaseClient();
        TestUser user = dbClient.WithUser();
        TestCategory category = user.WithCategory();
        dbClient.Save();

        CategoryClient apiClient = new(Integration.GetHttpClient(), Console.WriteLine);
        apiClient.SetUser(user);

        CategoryClient.Category? apiCategory = (await apiClient.GetById(category.Id)).IsSuccess().Content;
        Assert.That(apiCategory, Is.Not.Null);
        Assert.That(apiCategory.Id, Is.EqualTo(category.Id));
        Assert.That(apiCategory.Name, Is.EqualTo(category.Name));
        Assert.That(apiCategory.PaymentTypeId, Is.EqualTo((int)category.PaymentType));
    }

    [Test]
    public async Task CreateTest()
    {
        DatabaseClient dbClient = Integration.GetDatabaseClient();
        TestUser user = dbClient.WithUser();
        dbClient.Save();

        TestCategory category = user.WithCategory();

        CategoryClient apiClient = new(Integration.GetHttpClient(), Console.WriteLine);
        apiClient.SetUser(user);

        var request = new CategoryClient.CreateCategoryRequest
        {
            Name = category.Name,
            PaymentTypeId = category.PaymentType,
            Color = "#606217",
            Order = 217,
            ParentId = null,
        };
        int createdCategoryId = (await apiClient.Create(request)).IsSuccess().Content;
        var dbCategory = dbClient.CreateApplicationDbContext().Categories.SingleOrDefault(x => x.UserId == user.Id && x.Id == createdCategoryId);
        Assert.That(dbCategory, Is.Not.Null);
        Assert.That(dbCategory.Name, Is.EqualTo(request.Name));
        Assert.That(dbCategory.TypeId, Is.EqualTo(request.PaymentTypeId));
        Assert.That(dbCategory.Color, Is.EqualTo(request.Color));
        Assert.That(dbCategory.Order, Is.EqualTo(request.Order));
        Assert.That(dbCategory.ParentId, Is.EqualTo(request.ParentId));
    }

    [Test]
    public async Task DeleteTest()
    {
        DatabaseClient dbClient = Integration.GetDatabaseClient();
        TestUser user = dbClient.WithUser();
        TestCategory category = user.WithCategory();
        dbClient.Save();

        CategoryClient apiClient = new(Integration.GetHttpClient(), Console.WriteLine);
        apiClient.SetUser(user);

        (await apiClient.Delete(category.Id)).IsSuccess();
        var dbCategory = dbClient.CreateApplicationDbContext().Categories.Single(x => x.UserId == user.Id && x.Id == category.Id);
        Assert.That(dbCategory.IsDeleted, Is.EqualTo(true));
    }
}
