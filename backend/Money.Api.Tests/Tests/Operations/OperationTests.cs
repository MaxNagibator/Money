using Microsoft.EntityFrameworkCore;
using Money.Api.Tests.TestTools;
using Money.Api.Tests.TestTools.Entities;
using Money.ApiClient;
using Money.Data;
using Money.Data.Entities;
using Money.Data.Extensions;

namespace Money.Api.Tests.Operations;

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

        TestOperation[] operations =
        [
            category.WithOperation(),
            category.WithOperation(),
            category.WithOperation(),
        ];

        _dbClient.Save();

        OperationClient.Operation[]? apiOperations = await _apiClient.Operation.Get().IsSuccessWithContent();
        Assert.That(apiOperations, Is.Not.Null);
        Assert.That(apiOperations, Has.Length.GreaterThanOrEqualTo(operations.Length));

        TestOperation[] testCategories = operations.ExceptBy(apiOperations.Select(x => x.Id), operation => operation.Id).ToArray();
        Assert.That(testCategories, Is.Not.Null);
        Assert.That(testCategories, Is.Empty);
    }

    [Test]
    public async Task GetByDateFromTest()
    {
        TestCategory category = _user.WithCategory();

        TestOperation[] operations =
        [
            category.WithOperation().SetDate(DateTime.Now.Date.AddMonths(1)),
            category.WithOperation(),
        ];

        _dbClient.Save();

        OperationClient.OperationFilterDto filter = new()
        {
            DateFrom = DateTime.Now.Date.AddMonths(1),
        };

        OperationClient.Operation[]? apiOperations = await _apiClient.Operation.Get(filter).IsSuccessWithContent();
        Assert.That(apiOperations, Is.Not.Null);
        Assert.That(apiOperations, Has.Length.EqualTo(1));
        Assert.That(apiOperations[0].Date, Is.GreaterThanOrEqualTo(DateTime.Now.Date));
    }

    [Test]
    public async Task GetByDateToTest()
    {
        TestCategory category = _user.WithCategory();

        TestOperation[] operations =
        [
            category.WithOperation().SetDate(DateTime.Now.Date.AddMonths(1)),
            category.WithOperation().SetDate(DateTime.Now.Date),
        ];

        _dbClient.Save();

        OperationClient.OperationFilterDto filter = new()
        {
            DateTo = DateTime.Now.Date.AddDays(1),
        };

        OperationClient.Operation[]? apiOperations = await _apiClient.Operation.Get(filter).IsSuccessWithContent();
        Assert.That(apiOperations, Is.Not.Null);
        Assert.That(apiOperations, Has.Length.EqualTo(1));
        Assert.That(apiOperations[0].Date, Is.GreaterThanOrEqualTo(DateTime.Now.Date));
        Assert.That(apiOperations[0].Date, Is.LessThan(DateTime.Now.Date.AddDays(1)));
    }

    [Test]
    public async Task GetByDateRangeTest()
    {
        TestCategory category = _user.WithCategory();
        category.WithOperation().SetDate(DateTime.Now.Date.AddDays(-1));
        category.WithOperation().SetDate(DateTime.Now.Date);
        category.WithOperation().SetDate(DateTime.Now.Date.AddDays(1));
        _dbClient.Save();

        OperationClient.OperationFilterDto filter = new()
        {
            DateFrom = DateTime.Now.Date.AddDays(-1),
            DateTo = DateTime.Now.Date.AddDays(2),
        };

        OperationClient.Operation[]? apiOperations = await _apiClient.Operation.Get(filter).IsSuccessWithContent();
        Assert.That(apiOperations, Is.Not.Null);
        Assert.That(apiOperations, Has.Length.EqualTo(3));
    }

    [Test]
    public async Task GetByCategoryIdsTest()
    {
        _user.WithCategory().WithOperation();
        TestCategory category = _user.WithCategory().WithOperation().Category;
        _dbClient.Save();

        OperationClient.OperationFilterDto filter = new()
        {
            CategoryIds = [category.Id],
        };

        OperationClient.Operation[]? apiOperations = await _apiClient.Operation.Get(filter).IsSuccessWithContent();
        Assert.That(apiOperations, Is.Not.Null);
        Assert.That(apiOperations, Has.Length.EqualTo(1));
    }

    [Test]
    public async Task GetByCommentTest()
    {
        TestCategory category = _user.WithCategory();
        TestOperation operation = category.WithOperation().SetComment("ochen vazhniy platezh");
        category.WithOperation();
        _dbClient.Save();

        OperationClient.OperationFilterDto filter = new()
        {
            Comment = operation.Comment[..10],
        };

        OperationClient.Operation[]? apiOperations = await _apiClient.Operation.Get(filter).IsSuccessWithContent();
        Assert.That(apiOperations, Is.Not.Null);
        Assert.That(apiOperations, Has.Length.EqualTo(1));
        Assert.That(apiOperations[0].Comment, Is.EqualTo(operation.Comment));
    }

    [Test]
    public async Task GetByPlaceTest()
    {
        TestPlace place = _user.WithPlace();
        TestCategory category = _user.WithCategory();
        TestOperation operation = category.WithOperation();
        operation.SetPlace(place);
        category.WithOperation();
        _dbClient.Save();

        OperationClient.OperationFilterDto filter = new()
        {
            Place = place.Name,
        };

        OperationClient.Operation[]? apiOperations = await _apiClient.Operation.Get(filter).IsSuccessWithContent();
        Assert.That(apiOperations, Is.Not.Null);
        Assert.That(apiOperations, Has.Length.EqualTo(1));
        Assert.That(apiOperations[0].Place, Is.EqualTo(place.Name));
    }

    [Test]
    public async Task GetByMultipleCategoryIdsTest()
    {
        TestCategory category1 = _user.WithCategory();
        TestCategory category2 = _user.WithCategory();
        category1.WithOperation();
        category2.WithOperation();
        _dbClient.Save();

        OperationClient.OperationFilterDto filter = new()
        {
            CategoryIds = [category1.Id, category2.Id],
        };

        OperationClient.Operation[]? apiOperations = await _apiClient.Operation.Get(filter).IsSuccessWithContent();
        Assert.That(apiOperations, Is.Not.Null);
        Assert.That(apiOperations, Has.Length.EqualTo(2));
    }

    [Test]
    public async Task GetNoOperationsTest()
    {
        _dbClient.Save();

        OperationClient.OperationFilterDto filter = new()
        {
            DateFrom = DateTime.Now.AddDays(1),
        };

        OperationClient.Operation[]? apiOperations = await _apiClient.Operation.Get(filter).IsSuccessWithContent();
        Assert.That(apiOperations, Is.Not.Null);
        Assert.That(apiOperations, Is.Empty);
    }

    [Test]
    public async Task GetByIdTest()
    {
        TestPlace place = _user.WithPlace();
        TestOperation operation = _user.WithOperation().SetPlace(place);
        _dbClient.Save();

        OperationClient.Operation? apiOperation = await _apiClient.Operation.GetById(operation.Id).IsSuccessWithContent();

        Assert.That(apiOperation, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(apiOperation.Id, Is.EqualTo(operation.Id));
            Assert.That(apiOperation.Comment, Is.EqualTo(operation.Comment));
            Assert.That(apiOperation.CategoryId, Is.EqualTo(operation.Category.Id));
            Assert.That(apiOperation.Date, Is.EqualTo(operation.Date));
            Assert.That(apiOperation.Place, Is.EqualTo(operation.Place.Name));
        });
    }

    [Test]
    public async Task CreateTest()
    {
        TestCategory category = _user.WithCategory();
        _dbClient.Save();

        TestOperation operation = _user.WithOperation();
        TestPlace place = _user.WithPlace();

        OperationClient.SaveRequest request = new()
        {
            CategoryId = category.Id,
            Sum = operation.Sum,
            Date = operation.Date,
            Comment = operation.Comment,
            Place = place.Name,
        };

        int createdId = await _apiClient.Operation.Create(request).IsSuccessWithContent();
        Operation? dbOperation = _dbClient.CreateApplicationDbContext().Operations.SingleOrDefault(_user.Id, createdId);
        Place? dbPlace = _dbClient.CreateApplicationDbContext().Places.FirstOrDefault(x => x.UserId == _user.Id && x.Name == place.Name);

        Assert.Multiple(() =>
        {
            Assert.That(dbOperation, Is.Not.Null);
            Assert.That(dbPlace, Is.Not.Null);
        });

        Assert.Multiple(() =>
        {
            Assert.That(dbOperation.Date, Is.EqualTo(request.Date));
            Assert.That(dbOperation.Sum, Is.EqualTo(request.Sum));
            Assert.That(dbOperation.Comment, Is.EqualTo(request.Comment));
            Assert.That(dbOperation.CategoryId, Is.EqualTo(request.CategoryId));
            Assert.That(dbPlace.Name, Is.EqualTo(request.Place));
        });
    }

    [Test]
    public async Task UpdateTest()
    {
        TestOperation operation = _user.WithOperation();
        TestCategory updatedCategory = _user.WithCategory();
        _dbClient.Save();

        TestPlace place = _user.WithPlace();
        TestOperation updatedOperation = _user.WithOperation();

        OperationClient.SaveRequest request = new()
        {
            Comment = updatedOperation.Comment,
            CategoryId = updatedCategory.Id,
            Date = updatedOperation.Date,
            Place = place.Name,
            Sum = updatedOperation.Sum,
        };

        await _apiClient.Operation.Update(operation.Id, request).IsSuccess();
        Operation? dbOperation = _dbClient.CreateApplicationDbContext().Operations.SingleOrDefault(_user.Id, operation.Id);
        Place? dbPlace = _dbClient.CreateApplicationDbContext().Places.FirstOrDefault(x => x.UserId == _user.Id && x.Name == place.Name);

        Assert.Multiple(() =>
        {
            Assert.That(dbOperation, Is.Not.Null);
            Assert.That(dbPlace, Is.Not.Null);
        });

        Assert.Multiple(() =>
        {
            Assert.That(dbOperation.Date, Is.EqualTo(request.Date));
            Assert.That(dbOperation.Sum, Is.EqualTo(request.Sum));
            Assert.That(dbOperation.Comment, Is.EqualTo(request.Comment));
            Assert.That(dbOperation.CategoryId, Is.EqualTo(request.CategoryId));
            Assert.That(dbPlace.Name, Is.EqualTo(request.Place));
        });
    }

    [Test]
    public async Task UpdateBatchTest()
    {
        TestCategory initialCategory = _user.WithCategory();

        TestOperation[] operationsToUpdate =
        [
            initialCategory.WithOperation(),
            initialCategory.WithOperation(),
            initialCategory.WithOperation(),
        ];

        TestCategory targetCategory = _user.WithCategory();
        _dbClient.Save();

        OperationClient.UpdateOperationsBatchRequest updateRequest = new()
        {
            OperationIds = operationsToUpdate.Select(x => x.Id).ToList(),
            CategoryId = targetCategory.Id,
        };

        OperationClient.Operation[]? updatedOperations = await _apiClient.Operation.UpdateBatch(updateRequest).IsSuccessWithContent();

        List<Operation> targetCategoryOperations = await _dbClient.CreateApplicationDbContext()
            .Operations
            .IsUserEntity(_user.Id)
            .Where(x => x.CategoryId == targetCategory.Id)
            .ToListAsync();

        Assert.That(targetCategoryOperations, Has.Count.EqualTo(operationsToUpdate.Length));

        foreach (Operation operation in targetCategoryOperations)
        {
            Assert.That(operation.CategoryId, Is.EqualTo(targetCategory.Id));
        }

        List<Operation> initialCategoryOperations = await _dbClient.CreateApplicationDbContext()
            .Operations
            .IsUserEntity(_user.Id)
            .Where(x => x.CategoryId == initialCategory.Id)
            .ToListAsync();

        Assert.Multiple(() =>
        {
            Assert.That(initialCategoryOperations, Has.Count.EqualTo(0));
            Assert.That(updatedOperations, Has.Length.EqualTo(operationsToUpdate.Length));
        });

        foreach (OperationClient.Operation updatedOperation in updatedOperations)
        {
            Assert.That(updatedOperation.CategoryId, Is.EqualTo(targetCategory.Id));
        }

        List<Operation> allUserOperations = await _dbClient.CreateApplicationDbContext()
            .Operations
            .IsUserEntity(_user.Id)
            .ToListAsync();

        Assert.That(allUserOperations, Has.Count.EqualTo(operationsToUpdate.Length));
    }

    [Test]
    public async Task DeleteTest()
    {
        TestOperation operation = _user.WithOperation();
        _dbClient.Save();

        await _apiClient.Operation.Delete(operation.Id).IsSuccess();

        await using ApplicationDbContext context = _dbClient.CreateApplicationDbContext();

        Operation? dbOperation = context.Operations.SingleOrDefault(_user.Id, operation.Id);

        Assert.That(dbOperation, Is.Null);

        dbOperation = context.Operations
            .IgnoreQueryFilters()
            .SingleOrDefault(_user.Id, operation.Id);

        Assert.That(dbOperation, Is.Not.Null);
        Assert.That(dbOperation.IsDeleted, Is.EqualTo(true));
    }

    [Test]
    public async Task RestoreTest()
    {
        TestOperation operation = _user.WithOperation().SetIsDeleted();
        _dbClient.Save();

        await _apiClient.Operation.Restore(operation.Id).IsSuccess();

        await using ApplicationDbContext context = _dbClient.CreateApplicationDbContext();

        Operation? dbOperation = context.Operations.SingleOrDefault(_user.Id, operation.Id);
        Assert.That(dbOperation, Is.Not.Null);
        Assert.That(dbOperation.IsDeleted, Is.EqualTo(false));
    }
}
