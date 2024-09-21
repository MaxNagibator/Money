using Microsoft.EntityFrameworkCore;
using Money.Api.Tests.ApiClient;
using Money.Api.Tests.TestTools;
using Money.Api.Tests.TestTools.Entities;
using Money.Data;
using Money.Data.Entities;
using Money.Data.Extensions;

namespace Money.Api.Tests;

public class CategoryTests
{
    private DatabaseClient _dbClient;
    private TestUser _user;
    private CategoryClient _apiClient;

    [SetUp]
    public void Setup()
    {
        _dbClient = Integration.GetDatabaseClient();
        _user = _dbClient.WithUser();
        _apiClient = new CategoryClient(Integration.GetHttpClient(), Console.WriteLine);
        _apiClient.SetUser(_user);
    }

    [Test]
    public async Task GetTest()
    {
        TestCategory[] categories =
        [
            _user.WithCategory(),
            _user.WithCategory(),
            _user.WithCategory()
        ];

        _dbClient.Save();

        CategoryClient.Category[]? apiCategories = await _apiClient.Get(1).IsSuccessWithContent();
        Assert.That(apiCategories, Is.Not.Null);
        Assert.That(apiCategories.Count, Is.EqualTo(3));

        TestCategory[] testCategories = categories.ExceptBy(apiCategories.Select(x => x.Id), category => category.Id).ToArray();
        Assert.That(testCategories, Is.Not.Null);
        Assert.That(testCategories, Is.Empty);
    }

    [Test]
    public async Task GetByIdTest()
    {
        TestCategory category = _user.WithCategory();
        _dbClient.Save();

        CategoryClient.Category? apiCategory = await _apiClient.GetById(category.Id).IsSuccessWithContent();

        Assert.That(apiCategory, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(apiCategory.Id, Is.EqualTo(category.Id));
            Assert.That(apiCategory.Name, Is.EqualTo(category.Name));
            Assert.That(apiCategory.PaymentTypeId, Is.EqualTo((int)category.PaymentType));
        });
    }

    [Test]
    public async Task CreateTest()
    {
        _dbClient.Save();

        TestCategory category = _user.WithCategory();

        CategoryClient.CreateCategoryRequest request = new()
        {
            Name = category.Name,
            PaymentTypeId = category.PaymentType,
            Color = "#606217",
            Order = 217,
            ParentId = null
        };

        int createdCategoryId = await _apiClient.Create(request).IsSuccessWithContent();
        Category? dbCategory = _dbClient.CreateApplicationDbContext().Categories.SingleOrDefault(_user.Id, createdCategoryId);

        Assert.That(dbCategory, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(dbCategory.Name, Is.EqualTo(request.Name));
            Assert.That(dbCategory.TypeId, Is.EqualTo(request.PaymentTypeId));
            Assert.That(dbCategory.Color, Is.EqualTo(request.Color));
            Assert.That(dbCategory.Order, Is.EqualTo(request.Order));
            Assert.That(dbCategory.ParentId, Is.EqualTo(request.ParentId));
        });
    }

    [Test]
    public async Task DeleteTest()
    {
        TestCategory category = _user.WithCategory();
        _dbClient.Save();

        await _apiClient.Delete(category.Id).IsSuccess();

        await using ApplicationDbContext context = _dbClient.CreateApplicationDbContext();

        Category? dbCategory = context.Categories
            .SingleOrDefault(_user.Id, category.Id);

        Assert.That(dbCategory, Is.Null);

        dbCategory = context.Categories
            .IgnoreQueryFilters()
            .SingleOrDefault(_user.Id, category.Id);

        Assert.That(dbCategory, Is.Not.Null);
        Assert.That(dbCategory.IsDeleted, Is.EqualTo(true));
    }
}
