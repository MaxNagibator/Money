﻿using Microsoft.EntityFrameworkCore;
using Money.ApiClient;
using Money.Business.Enums;
using Money.Data.Extensions;

namespace Money.Api.Tests.Categories;

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
        _apiClient = new(Integration.GetHttpClient(), Console.WriteLine);
        _apiClient.SetUser(_user);
    }

    [Test]
    [TestCase(OperationTypes.Costs)]
    [TestCase(OperationTypes.Income)]
    public async Task GetTest(OperationTypes operationType)
    {
        TestCategory[] categories =
        [
            _user.WithCategory().SetOperationType(operationType),
            _user.WithCategory().SetOperationType(operationType),
            _user.WithCategory().SetOperationType(operationType),
        ];

        _dbClient.Save();

        var apiCategories = await _apiClient.Categories.Get((int)operationType).IsSuccessWithContent();
        Assert.That(apiCategories, Is.Not.Null);
        Assert.That(apiCategories.Count, Is.GreaterThanOrEqualTo(3));

        var testCategories = categories.ExceptBy(apiCategories.Select(x => x.Id), category => category.Id).ToArray();
        Assert.That(testCategories, Is.Not.Null);
        Assert.That(testCategories, Is.Empty);
    }

    [Test]
    public async Task GetByIdTest()
    {
        var category = _user.WithCategory();
        _dbClient.Save();

        var apiCategory = await _apiClient.Categories.GetById(category.Id).IsSuccessWithContent();

        Assert.That(apiCategory, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(apiCategory.Id, Is.EqualTo(category.Id));
            Assert.That(apiCategory.Name, Is.EqualTo(category.Name));
            Assert.That(apiCategory.OperationTypeId, Is.EqualTo((int)category.OperationType));
        });
    }

    [Test]
    public async Task CreateTest()
    {
        _dbClient.Save();

        var category = _user.WithCategory();

        var request = new CategoriesClient.SaveRequest
        {
            Name = category.Name,
            OperationTypeId = (int)category.OperationType,
            Color = "#606217",
            Order = 217,
            ParentId = null,
        };

        var createdCategoryId = await _apiClient.Categories.Create(request).IsSuccessWithContent();
        var dbCategory = await _dbClient.CreateApplicationDbContext().Categories.FirstOrDefaultAsync(_user.Id, createdCategoryId);

        Assert.That(dbCategory, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(dbCategory.Name, Is.EqualTo(request.Name));
            Assert.That(dbCategory.TypeId, Is.EqualTo(request.OperationTypeId));
            Assert.That(dbCategory.Color, Is.EqualTo(request.Color));
            Assert.That(dbCategory.Order, Is.EqualTo(request.Order));
            Assert.That(dbCategory.ParentId, Is.EqualTo(request.ParentId));
        });
    }

    [Test]
    public async Task UpdateTest()
    {
        var category = _user.WithCategory();
        _dbClient.Save();

        var request = new CategoriesClient.SaveRequest
        {
            Name = category.Name,
            OperationTypeId = (int)category.OperationType,
            Color = "#606217",
            Order = 217,
            ParentId = null,
        };

        await _apiClient.Categories.Update(category.Id, request).IsSuccess();
        var dbCategory = await _dbClient.CreateApplicationDbContext().Categories.FirstOrDefaultAsync(_user.Id, category.Id);

        Assert.That(dbCategory, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(dbCategory.Name, Is.EqualTo(request.Name));
            Assert.That(dbCategory.TypeId, Is.EqualTo(request.OperationTypeId));
            Assert.That(dbCategory.Color, Is.EqualTo(request.Color));
            Assert.That(dbCategory.Order, Is.EqualTo(request.Order));
            Assert.That(dbCategory.ParentId, Is.EqualTo(request.ParentId));
        });
    }

    [Test]
    [TestCase(OperationTypes.Costs)]
    [TestCase(OperationTypes.Income)]
    public async Task UpdateRecursiveFailTest(OperationTypes operationType)
    {
        var category1 = _user.WithCategory().SetOperationType(operationType);
        var category2 = _user.WithCategory().SetOperationType(operationType);
        var category3 = _user.WithCategory().SetOperationType(operationType);
        category2.SetParent(category1);
        category3.SetParent(category2);
        _dbClient.Save();

        var request = new CategoriesClient.SaveRequest
        {
            Name = category1.Name,
            OperationTypeId = (int)category1.OperationType,
            ParentId = category3.Id,
        };

        await _apiClient.Categories.Update(category1.Id, request).IsBadRequest();

        request = new()
        {
            Name = category2.Name,
            OperationTypeId = (int)category2.OperationType,
            ParentId = category3.Id,
        };

        await _apiClient.Categories.Update(category2.Id, request).IsBadRequest();
    }

    [Test]
    public async Task DeleteTest()
    {
        var category = _user.WithCategory();
        _dbClient.Save();

        await _apiClient.Categories.Delete(category.Id).IsSuccess();

        await using var context = _dbClient.CreateApplicationDbContext();

        var dbCategory = await context.Categories
            .FirstOrDefaultAsync(_user.Id, category.Id);

        Assert.That(dbCategory, Is.Null);

        dbCategory = await context.Categories
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(_user.Id, category.Id);

        Assert.That(dbCategory, Is.Not.Null);
        Assert.That(dbCategory.IsDeleted, Is.EqualTo(true));
    }

    [Test]
    public async Task RestoreTest()
    {
        var category = _user.WithCategory().SetIsDeleted();
        _dbClient.Save();

        await _apiClient.Categories.Restore(category.Id).IsSuccess();

        await using var context = _dbClient.CreateApplicationDbContext();

        var dbCategory = await context.Categories.FirstOrDefaultAsync(_user.Id, category.Id);
        Assert.That(dbCategory, Is.Not.Null);
        Assert.That(dbCategory.IsDeleted, Is.EqualTo(false));
    }

    [Test]
    public async Task RestoreFailWhenNotDeletedTest()
    {
        var category = _user.WithCategory();
        _dbClient.Save();

        await _apiClient.Categories.Restore(category.Id).IsBadRequest();
    }

    [Test]
    public async Task RestoreFailWhenNotExistTest()
    {
        _dbClient.Save();

        await _apiClient.Categories.Restore(-1).IsNotFound();
    }

    [Test]
    public async Task RestoreFailWhenParentDeletedTest()
    {
        var parent = _user.WithCategory();
        var child = _user.WithCategory();
        child.SetParent(parent);
        _dbClient.Save();

        await _apiClient.Categories.Delete(child.Id).IsSuccess();
        await _apiClient.Categories.Delete(parent.Id).IsSuccess();

        await _apiClient.Categories.Restore(child.Id).IsBadRequest();
    }
}
