using Microsoft.EntityFrameworkCore;
using Money.ApiClient;
using Money.Business.Enums;
using Money.Data.Extensions;

namespace Money.Api.Tests.RegularOperations;

public class RegularOperationTests
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

        TestRegularOperation[] operations =
        [
            category.WithRegularOperation(),
            category.WithRegularOperation(),
            category.WithRegularOperation(),
        ];

        _dbClient.Save();

        var apiOperations = await _apiClient.RegularOperation.Get().IsSuccessWithContent();
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
        var operation = _user.WithRegularOperation().SetPlace(place);
        _dbClient.Save();

        var apiOperation = await _apiClient.RegularOperation.GetById(operation.Id).IsSuccessWithContent();

        Assert.That(apiOperation, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(apiOperation.Id, Is.EqualTo(operation.Id));
            Assert.That(apiOperation.Comment, Is.EqualTo(operation.Comment));
            Assert.That(apiOperation.CategoryId, Is.EqualTo(operation.Category.Id));
            Assert.That(apiOperation.Name, Is.EqualTo(operation.Name));
            Assert.That(apiOperation.Place, Is.EqualTo(operation.Place?.Name));
            Assert.That(apiOperation.TimeTypeId, Is.EqualTo((int)operation.TimeType));
            Assert.That(apiOperation.TimeValue, Is.EqualTo(operation.TimeValue));
            Assert.That(apiOperation.DateFrom, Is.EqualTo(operation.DateFrom));
            Assert.That(apiOperation.DateTo, Is.EqualTo(operation.DateTo));
            Assert.That(apiOperation.RunTime, Is.EqualTo(operation.RunTime));
        });
    }

    [Test]
    public async Task CreateTest()
    {
        var category = _user.WithCategory();
        _dbClient.Save();

        var operation = _user.WithRegularOperation();
        var place = _user.WithPlace();

        var request = new RegularOperationClient.SaveRequest
        {
            CategoryId = category.Id,
            Sum = operation.Sum,
            Name = operation.Name,
            Comment = operation.Comment,
            Place = place.Name,
            DateFrom = operation.DateFrom,
            DateTo = operation.DateTo,
            TimeTypeId = (int)operation.TimeType,
            TimeValue = operation.TimeValue,
        };

        var createdId = await _apiClient.RegularOperation.Create(request).IsSuccessWithContent();
        var dbOperation = _dbClient.CreateApplicationDbContext().RegularOperations.SingleOrDefault(_user.Id, createdId);
        var dbPlace = _dbClient.CreateApplicationDbContext().Places.FirstOrDefault(x => x.UserId == _user.Id && x.Name == place.Name);

        Assert.Multiple(() =>
        {
            Assert.That(dbOperation, Is.Not.Null);
            Assert.That(dbPlace, Is.Not.Null);
        });

        Assert.Multiple(() =>
        {
            Assert.That(dbOperation.Name, Is.EqualTo(request.Name));
            Assert.That(dbOperation.Sum, Is.EqualTo(request.Sum));
            Assert.That(dbOperation.Comment, Is.EqualTo(request.Comment));
            Assert.That(dbOperation.CategoryId, Is.EqualTo(request.CategoryId));
            Assert.That(dbPlace.Name, Is.EqualTo(request.Place));
            Assert.That(dbOperation.TimeTypeId, Is.EqualTo(request.TimeTypeId));
            Assert.That(dbOperation.TimeValue, Is.EqualTo(request.TimeValue));
            Assert.That(dbOperation.DateFrom, Is.EqualTo(request.DateFrom));
            Assert.That(dbOperation.DateTo, Is.EqualTo(request.DateTo));
            Assert.That(dbOperation.RunTime, Is.Not.Null);
        });
    }

    [Test]
    public async Task UpdateTest()
    {
        var operation = _user.WithRegularOperation();
        var updatedCategory = _user.WithCategory();
        _dbClient.Save();

        var place = _user.WithPlace();

        var request = new RegularOperationClient.SaveRequest
        {
            Comment = "updateComment",
            CategoryId = updatedCategory.Id,
            Name = "updateName",
            Place = place.Name,
            DateFrom = DateTime.Now.Date.AddDays(1),
            DateTo = DateTime.Now.Date.AddDays(1),
            TimeTypeId = (int)RegularOperationTimeTypes.EveryWeek,
            TimeValue = 3,
        };

        await _apiClient.RegularOperation.Update(operation.Id, request).IsSuccess();
        var dbOperation = _dbClient.CreateApplicationDbContext().RegularOperations.SingleOrDefault(_user.Id, operation.Id);
        var dbPlace = _dbClient.CreateApplicationDbContext().Places.FirstOrDefault(x => x.UserId == _user.Id && x.Name == place.Name);

        Assert.Multiple(() =>
        {
            Assert.That(dbOperation, Is.Not.Null);
            Assert.That(dbPlace, Is.Not.Null);
        });

        Assert.Multiple(() =>
        {
            Assert.That(dbOperation.Name, Is.EqualTo(request.Name));
            Assert.That(dbOperation.Sum, Is.EqualTo(request.Sum));
            Assert.That(dbOperation.Comment, Is.EqualTo(request.Comment));
            Assert.That(dbOperation.CategoryId, Is.EqualTo(request.CategoryId));
            Assert.That(dbPlace.Name, Is.EqualTo(request.Place));
            Assert.That(dbOperation.TimeTypeId, Is.EqualTo(request.TimeTypeId));
            Assert.That(dbOperation.TimeValue, Is.EqualTo(request.TimeValue));
            Assert.That(dbOperation.DateFrom, Is.EqualTo(request.DateFrom));
            Assert.That(dbOperation.DateTo, Is.EqualTo(request.DateTo));
            Assert.That(dbOperation.RunTime, Is.Not.Null);
        });
    }

    [Test]
    public async Task DeleteTest()
    {
        var operation = _user.WithRegularOperation();
        _dbClient.Save();

        await _apiClient.RegularOperation.Delete(operation.Id).IsSuccess();

        await using var context = _dbClient.CreateApplicationDbContext();

        var dbOperation = context.RegularOperations.SingleOrDefault(_user.Id, operation.Id);

        Assert.That(dbOperation, Is.Null);

        dbOperation = context.RegularOperations
            .IgnoreQueryFilters()
            .SingleOrDefault(_user.Id, operation.Id);

        Assert.That(dbOperation, Is.Not.Null);
        Assert.That(dbOperation.IsDeleted, Is.EqualTo(true));
    }

    [Test]
    public async Task RestoreTest()
    {
        var operation = _user.WithRegularOperation().SetIsDeleted();
        _dbClient.Save();

        await _apiClient.RegularOperation.Restore(operation.Id).IsSuccess();

        await using var context = _dbClient.CreateApplicationDbContext();

        var dbOperation = context.RegularOperations.SingleOrDefault(_user.Id, operation.Id);
        Assert.That(dbOperation, Is.Not.Null);
        Assert.That(dbOperation.IsDeleted, Is.EqualTo(false));
    }

    [Test]
    [TestCase(RegularOperationTimeTypes.EveryDay, null)]
    [TestCase(RegularOperationTimeTypes.EveryWeek, 1)]
    [TestCase(RegularOperationTimeTypes.EveryWeek, 2)]
    [TestCase(RegularOperationTimeTypes.EveryMonth, 1)]
    [TestCase(RegularOperationTimeTypes.EveryMonth, 15)]
    [TestCase(RegularOperationTimeTypes.EveryMonth, 31)]
    public async Task RunTimeTest(RegularOperationTimeTypes timeType, int? timeValue)
    {
        var category = _user.WithCategory();
        _dbClient.Save();

        var operation = _user.WithRegularOperation();

        var request = new RegularOperationClient.SaveRequest
        {
            CategoryId = category.Id,
            Sum = operation.Sum,
            Name = operation.Name,
            Comment = operation.Comment,
            DateFrom = DateTime.Now.Date.AddDays(-1),
            DateTo = DateTime.Now.Date.AddDays(217),
            TimeTypeId = (int)timeType,
            TimeValue = timeValue,
        };

        DateTime runTime;

        switch (timeType)
        {
            case RegularOperationTimeTypes.EveryDay:
                runTime = DateTime.Now.Date.AddDays(1);
                break;

            case RegularOperationTimeTypes.EveryWeek:
            {
                var daysToAdd = (timeValue!.Value - (int)DateTime.Now.Date.AddDays(1).DayOfWeek + 7) % 7;
                runTime = DateTime.Now.Date.AddDays(1).AddDays(daysToAdd);
                break;
            }

            case RegularOperationTimeTypes.EveryMonth:
            {
                runTime = new(DateTime.Now.Year, DateTime.Now.Month, 1);

                if (DateTime.Now.Day >= timeValue)
                {
                    runTime = runTime.AddMonths(1);
                }

                // если выбрали 31, то будет последний день месяца, 28 или 30 например
                var nextDt = runTime.AddDays(timeValue!.Value - 1);

                runTime = runTime.Month < nextDt.Month
                    ? runTime.AddMonths(1).AddDays(-1)
                    : nextDt;

                break;
            }

            default:
                throw new ArgumentOutOfRangeException(nameof(timeType), timeType, null);
        }

        var createdId = await _apiClient.RegularOperation.Create(request).IsSuccessWithContent();
        var dbOperation = _dbClient.CreateApplicationDbContext().RegularOperations.SingleOrDefault(_user.Id, createdId);
        Assert.That(dbOperation, Is.Not.Null);
        Assert.That(dbOperation.RunTime, Is.Not.Null);
        Assert.That(dbOperation.RunTime.Value, Is.EqualTo(runTime));
    }
}
