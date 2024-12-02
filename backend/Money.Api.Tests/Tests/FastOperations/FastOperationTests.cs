using Microsoft.EntityFrameworkCore;
using Money.Api.Tests.TestTools;
using Money.Api.Tests.TestTools.Entities;
using Money.ApiClient;
using Money.Data;
using Money.Data.Entities;
using Money.Data.Extensions;

namespace Money.Api.Tests.FastOperations;

public class FastOperationTests
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
        TestCategory category = _user.WithCategory();

        TestFastOperation[] operations =
        [
            category.WithFastOperation(),
            category.WithFastOperation(),
            category.WithFastOperation(),
        ];

        _dbClient.Save();

        FastOperationClient.FastOperation[]? apiOperations = await _apiClient.FastOperation.Get().IsSuccessWithContent();
        Assert.That(apiOperations, Is.Not.Null);
        Assert.That(apiOperations, Has.Length.GreaterThanOrEqualTo(operations.Length));

        TestFastOperation[] testOperations = operations.ExceptBy(apiOperations.Select(x => x.Id), operation => operation.Id).ToArray();
        Assert.That(testOperations, Is.Not.Null);
        Assert.That(testOperations, Is.Empty);
    }

    [Test]
    public async Task GetByIdTest()
    {
        TestPlace place = _user.WithPlace();
        TestFastOperation operation = _user.WithFastOperation().SetOrder(217).SetPlace(place);
        _dbClient.Save();

        FastOperationClient.FastOperation? apiOperation = await _apiClient.FastOperation.GetById(operation.Id).IsSuccessWithContent();

        Assert.That(apiOperation, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(apiOperation.Id, Is.EqualTo(operation.Id));
            Assert.That(apiOperation.Comment, Is.EqualTo(operation.Comment));
            Assert.That(apiOperation.CategoryId, Is.EqualTo(operation.Category.Id));
            Assert.That(apiOperation.Name, Is.EqualTo(operation.Name));
            Assert.That(apiOperation.Order, Is.EqualTo(operation.Order));
            Assert.That(apiOperation.Place, Is.EqualTo(operation.Place.Name));
        });
    }

    [Test]
    public async Task CreateTest()
    {
        TestCategory category = _user.WithCategory();
        _dbClient.Save();

        TestFastOperation operation = _user.WithFastOperation();
        TestPlace place = _user.WithPlace();

        FastOperationClient.SaveRequest request = new()
        {
            CategoryId = category.Id,
            Sum = operation.Sum,
            Name = operation.Name,
            Order = operation.Order,
            Comment = operation.Comment,
            Place = place.Name,
        };

        int createdId = await _apiClient.FastOperation.Create(request).IsSuccessWithContent();
        FastOperation? dbOperation = _dbClient.CreateApplicationDbContext().FastOperations.SingleOrDefault(_user.Id, createdId);
        Place? dbPlace = _dbClient.CreateApplicationDbContext().Places.FirstOrDefault(x => x.UserId == _user.Id && x.Name == place.Name);

        Assert.Multiple(() =>
        {
            Assert.That(dbOperation, Is.Not.Null);
            Assert.That(dbPlace, Is.Not.Null);
        });

        Assert.Multiple(() =>
        {
            Assert.That(dbOperation.Name, Is.EqualTo(request.Name));
            Assert.That(dbOperation.Order, Is.EqualTo(request.Order));
            Assert.That(dbOperation.Sum, Is.EqualTo(request.Sum));
            Assert.That(dbOperation.Comment, Is.EqualTo(request.Comment));
            Assert.That(dbOperation.CategoryId, Is.EqualTo(request.CategoryId));
            Assert.That(dbPlace.Name, Is.EqualTo(request.Place));
        });
    }

    [Test]
    public async Task UpdateTest()
    {
        TestFastOperation operation = _user.WithFastOperation();
        TestCategory updatedCategory = _user.WithCategory();
        _dbClient.Save();

        TestPlace place = _user.WithPlace();
        TestFastOperation updatedOperation = _user.WithFastOperation().SetOrder(10);

        FastOperationClient.SaveRequest request = new()
        {
            Comment = updatedOperation.Comment,
            CategoryId = updatedCategory.Id,
            Name = operation.Name,
            Order = operation.Order,
            Place = place.Name,
            Sum = updatedOperation.Sum,
        };

        await _apiClient.FastOperation.Update(operation.Id, request).IsSuccess();
        FastOperation? dbOperation = _dbClient.CreateApplicationDbContext().FastOperations.SingleOrDefault(_user.Id, operation.Id);
        Place? dbPlace = _dbClient.CreateApplicationDbContext().Places.FirstOrDefault(x => x.UserId == _user.Id && x.Name == place.Name);

        Assert.Multiple(() =>
        {
            Assert.That(dbOperation, Is.Not.Null);
            Assert.That(dbPlace, Is.Not.Null);
        });

        Assert.Multiple(() =>
        {
            Assert.That(dbOperation.Name, Is.EqualTo(request.Name));
            Assert.That(dbOperation.Order, Is.EqualTo(request.Order));
            Assert.That(dbOperation.Sum, Is.EqualTo(request.Sum));
            Assert.That(dbOperation.Comment, Is.EqualTo(request.Comment));
            Assert.That(dbOperation.CategoryId, Is.EqualTo(request.CategoryId));
            Assert.That(dbPlace.Name, Is.EqualTo(request.Place));
        });
    }

    [Test]
    public async Task DeleteTest()
    {
        TestFastOperation operation = _user.WithFastOperation();
        _dbClient.Save();

        await _apiClient.FastOperation.Delete(operation.Id).IsSuccess();

        await using ApplicationDbContext context = _dbClient.CreateApplicationDbContext();

        FastOperation? dbOperation = context.FastOperations.SingleOrDefault(_user.Id, operation.Id);

        Assert.That(dbOperation, Is.Null);

        dbOperation = context.FastOperations
            .IgnoreQueryFilters()
            .SingleOrDefault(_user.Id, operation.Id);

        Assert.That(dbOperation, Is.Not.Null);
        Assert.That(dbOperation.IsDeleted, Is.EqualTo(true));
    }

    [Test]
    public async Task RestoreTest()
    {
        TestFastOperation operation = _user.WithFastOperation().SetIsDeleted();
        _dbClient.Save();

        await _apiClient.FastOperation.Restore(operation.Id).IsSuccess();

        await using ApplicationDbContext context = _dbClient.CreateApplicationDbContext();

        FastOperation? dbOperation = context.FastOperations.SingleOrDefault(_user.Id, operation.Id);
        Assert.That(dbOperation, Is.Not.Null);
        Assert.That(dbOperation.IsDeleted, Is.EqualTo(false));
    }
}
