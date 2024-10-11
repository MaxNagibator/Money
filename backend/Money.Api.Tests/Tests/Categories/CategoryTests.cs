using Microsoft.EntityFrameworkCore;
using Money.Api.Tests.TestTools;
using Money.Api.Tests.TestTools.Entities;
using Money.ApiClient;
using Money.Data;
using Money.Data.Entities;
using Money.Data.Extensions;

namespace Money.Api.Tests.Tests.Categories;

public class CategoryTests
{
    private DatabaseClient _dbClient;
    private TestUser _user;
    private MoneyClient _apiClient;

    [SetUp]
    public void Setup()
    {
        _dbClient = Integration.GetDatabaseClient();
        _user = _dbClient.WithUser();
        _apiClient = new MoneyClient(Integration.GetHttpClient(), Console.WriteLine);
        _apiClient.SetUser(_user);
    }

    [Test]
    public async Task GetTest()
    {
        TestCategory[] categories =
        [
            _user.WithCategory(),
            _user.WithCategory(),
            _user.WithCategory(),
        ];

        _dbClient.Save();

        CategoryClient.Category[]? apiCategories = await _apiClient.Category.Get(1).IsSuccessWithContent();
        Assert.That(apiCategories, Is.Not.Null);
        Assert.That(apiCategories.Count, Is.GreaterThanOrEqualTo(3));

        TestCategory[] testCategories = categories.ExceptBy(apiCategories.Select(x => x.Id), category => category.Id).ToArray();
        Assert.That(testCategories, Is.Not.Null);
        Assert.That(testCategories, Is.Empty);
    }

    [Test]
    public async Task GetByIdTest()
    {
        TestCategory category = _user.WithCategory();
        _dbClient.Save();

        CategoryClient.Category? apiCategory = await _apiClient.Category.GetById(category.Id).IsSuccessWithContent();

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

        CategoryClient.SaveRequest request = new()
        {
            Name = category.Name,
            PaymentTypeId = category.PaymentType,
            Color = "#606217",
            Order = 217,
            ParentId = null,
        };

        int createdCategoryId = await _apiClient.Category.Create(request).IsSuccessWithContent();
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
    public async Task UpdateTest()
    {
        TestCategory category = _user.WithCategory();
        _dbClient.Save();

        CategoryClient.SaveRequest request = new()
        {
            Name = category.Name,
            PaymentTypeId = category.PaymentType,
            Color = "#606217",
            Order = 217,
            ParentId = null,
        };

        await _apiClient.Category.Update(category.Id, request).IsSuccess();
        Category? dbCategory = _dbClient.CreateApplicationDbContext().Categories.SingleOrDefault(_user.Id, category.Id);

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
    public async Task UpdateRecursiveFailTest()
    {
        TestCategory category1 = _user.WithCategory();
        TestCategory category2 = _user.WithCategory();
        TestCategory category3 = _user.WithCategory();
        category2.SetParent(category1);
        category3.SetParent(category2);
        _dbClient.Save();

        CategoryClient.SaveRequest request = new()
        {
            Name = category1.Name,
            PaymentTypeId = category1.PaymentType,
            ParentId = category3.Id,
        };

        await _apiClient.Category.Update(category1.Id, request).IsBadRequest();

        request = new CategoryClient.SaveRequest
        {
            Name = category2.Name,
            PaymentTypeId = category2.PaymentType,
            ParentId = category3.Id,
        };

        await _apiClient.Category.Update(category2.Id, request).IsBadRequest();
    }

    [Test]
    public async Task DeleteTest()
    {
        TestCategory category = _user.WithCategory();
        _dbClient.Save();

        await _apiClient.Category.Delete(category.Id).IsSuccess();

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

    [Test]
    public async Task RestoreTest()
    {
        TestCategory category = _user.WithCategory().SetIsDeleted();
        _dbClient.Save();

        await _apiClient.Category.Restore(category.Id).IsSuccess();

        await using ApplicationDbContext context = _dbClient.CreateApplicationDbContext();

        Category? dbCategory = context.Categories.SingleOrDefault(_user.Id, category.Id);
        Assert.That(dbCategory, Is.Not.Null);
        Assert.That(dbCategory.IsDeleted, Is.EqualTo(false));
    }
}
