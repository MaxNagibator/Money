using Microsoft.EntityFrameworkCore;
using Money.ApiClient;
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
        _apiClient = new(Integration.GetHttpClient(), Console.WriteLine);
        _apiClient.SetUser(_user);
    }

    [Test]
    public async Task GetTest()
    {
        var category = _user.WithCategory();

        TestFastOperation[] operations =
        [
            category.WithFastOperation(),
            category.WithFastOperation(),
            category.WithFastOperation(),
        ];

        _dbClient.Save();

        var apiOperations = await _apiClient.FastOperation.Get().IsSuccessWithContent();
        Assert.That(apiOperations, Is.Not.Null);
        Assert.That(apiOperations, Has.Length.GreaterThanOrEqualTo(operations.Length));

        var testOperations = operations.ExceptBy(apiOperations.Select(x => x.Id), operation => operation.Id).ToArray();
        Assert.That(testOperations, Is.Not.Null);
        Assert.That(testOperations, Is.Empty);
    }

    [Test]
    public async Task GetByIdTest()
    {
        var place = _user.WithPlace();
        var operation = _user.WithFastOperation().SetOrder(217).SetPlace(place);
        _dbClient.Save();

        var apiOperation = await _apiClient.FastOperation.GetById(operation.Id).IsSuccessWithContent();

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
        var category = _user.WithCategory();
        _dbClient.Save();

        var operation = _user.WithFastOperation();
        var place = _user.WithPlace();

        var request = new FastOperationClient.SaveRequest
        {
            CategoryId = category.Id,
            Sum = operation.Sum,
            Name = operation.Name,
            Order = operation.Order,
            Comment = operation.Comment,
            Place = place.Name,
        };

        var createdId = await _apiClient.FastOperation.Create(request).IsSuccessWithContent();
        var dbOperation = _dbClient.CreateApplicationDbContext().FastOperations.SingleOrDefault(_user.Id, createdId);
        var dbPlace = _dbClient.CreateApplicationDbContext().Places.FirstOrDefault(x => x.UserId == _user.Id && x.Name == place.Name);

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
        var operation = _user.WithFastOperation();
        var updatedCategory = _user.WithCategory();
        _dbClient.Save();

        var place = _user.WithPlace();
        var updatedOperation = _user.WithFastOperation().SetOrder(10);

        var request = new FastOperationClient.SaveRequest
        {
            Comment = updatedOperation.Comment,
            CategoryId = updatedCategory.Id,
            Name = operation.Name,
            Order = operation.Order,
            Place = place.Name,
            Sum = updatedOperation.Sum,
        };

        await _apiClient.FastOperation.Update(operation.Id, request).IsSuccess();
        var dbOperation = _dbClient.CreateApplicationDbContext().FastOperations.SingleOrDefault(_user.Id, operation.Id);
        var dbPlace = _dbClient.CreateApplicationDbContext().Places.FirstOrDefault(x => x.UserId == _user.Id && x.Name == place.Name);

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
        var operation = _user.WithFastOperation();
        _dbClient.Save();

        await _apiClient.FastOperation.Delete(operation.Id).IsSuccess();

        await using var context = _dbClient.CreateApplicationDbContext();

        var dbOperation = context.FastOperations.SingleOrDefault(_user.Id, operation.Id);

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
        var operation = _user.WithFastOperation().SetIsDeleted();
        _dbClient.Save();

        await _apiClient.FastOperation.Restore(operation.Id).IsSuccess();

        await using var context = _dbClient.CreateApplicationDbContext();

        var dbOperation = context.FastOperations.SingleOrDefault(_user.Id, operation.Id);
        Assert.That(dbOperation, Is.Not.Null);
        Assert.That(dbOperation.IsDeleted, Is.EqualTo(false));
    }
}
