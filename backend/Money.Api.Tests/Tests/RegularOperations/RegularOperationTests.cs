using Microsoft.EntityFrameworkCore;
using Money.Api.Tests.TestTools;
using Money.Api.Tests.TestTools.Entities;
using Money.ApiClient;
using Money.Business.Enums;
using Money.Data;
using Money.Data.Entities;
using Money.Data.Extensions;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
        _apiClient = new MoneyClient(Integration.GetHttpClient(), Console.WriteLine);
        _apiClient.SetUser(_user);
    }

    [Test]
    public async Task GetTest()
    {
        TestCategory category = _user.WithCategory();

        TestRegularOperation[] operations =
        [
            category.WithRegularOperation(),
            category.WithRegularOperation(),
            category.WithRegularOperation(),
        ];

        _dbClient.Save();

        RegularOperationClient.RegularOperation[]? apiOperations = await _apiClient.RegularOperation.Get().IsSuccessWithContent();
        Assert.That(apiOperations, Is.Not.Null);
        Assert.That(apiOperations, Has.Length.GreaterThanOrEqualTo(operations.Length));

        TestRegularOperation[] testOperations = operations.ExceptBy(apiOperations.Select(x => x.Id), operation => operation.Id).ToArray();
        Assert.That(testOperations, Is.Not.Null);
        Assert.That(testOperations, Is.Empty);
    }

    [Test]
    public async Task GetByIdTest()
    {
        TestPlace place = _user.WithPlace();
        TestRegularOperation operation = _user.WithRegularOperation().SetPlace(place);
        _dbClient.Save();

        RegularOperationClient.RegularOperation? apiOperation = await _apiClient.RegularOperation.GetById(operation.Id).IsSuccessWithContent();

        Assert.That(apiOperation, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(apiOperation.Id, Is.EqualTo(operation.Id));
            Assert.That(apiOperation.Comment, Is.EqualTo(operation.Comment));
            Assert.That(apiOperation.CategoryId, Is.EqualTo(operation.Category.Id));
            Assert.That(apiOperation.Name, Is.EqualTo(operation.Name));
            Assert.That(apiOperation.Place, Is.EqualTo(operation.Place.Name));
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
        TestCategory category = _user.WithCategory();
        _dbClient.Save();

        TestRegularOperation operation = _user.WithRegularOperation();
        TestPlace place = _user.WithPlace();

        RegularOperationClient.SaveRequest request = new()
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

        int createdId = await _apiClient.RegularOperation.Create(request).IsSuccessWithContent();
        RegularOperation? dbOperation = _dbClient.CreateApplicationDbContext().RegularOperations.SingleOrDefault(_user.Id, createdId);
        Place? dbPlace = _dbClient.CreateApplicationDbContext().Places.FirstOrDefault(x => x.UserId == _user.Id && x.Name == place.Name);

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
        TestRegularOperation operation = _user.WithRegularOperation();
        TestCategory updatedCategory = _user.WithCategory();
        _dbClient.Save();

        TestPlace place = _user.WithPlace();

        RegularOperationClient.SaveRequest request = new()
        {
            Comment = "updateComment",
            CategoryId = updatedCategory.Id,
            Name = "updateName",
            Place = place.Name,
            DateFrom = DateTime.Now.Date.AddDays(1),
            DateTo = DateTime.Now.Date.AddDays(1),
            TimeTypeId = (int)RegularTaskTimeTypes.EveryWeek,
            TimeValue = 3,
        };

        await _apiClient.RegularOperation.Update(operation.Id, request).IsSuccess();
        RegularOperation? dbOperation = _dbClient.CreateApplicationDbContext().RegularOperations.SingleOrDefault(_user.Id, operation.Id);
        Place? dbPlace = _dbClient.CreateApplicationDbContext().Places.FirstOrDefault(x => x.UserId == _user.Id && x.Name == place.Name);

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
        TestRegularOperation operation = _user.WithRegularOperation();
        _dbClient.Save();

        await _apiClient.RegularOperation.Delete(operation.Id).IsSuccess();

        await using ApplicationDbContext context = _dbClient.CreateApplicationDbContext();

        RegularOperation? dbOperation = context.RegularOperations.SingleOrDefault(_user.Id, operation.Id);

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
        TestRegularOperation operation = _user.WithRegularOperation().SetIsDeleted();
        _dbClient.Save();

        await _apiClient.RegularOperation.Restore(operation.Id).IsSuccess();

        await using ApplicationDbContext context = _dbClient.CreateApplicationDbContext();

        RegularOperation? dbOperation = context.RegularOperations.SingleOrDefault(_user.Id, operation.Id);
        Assert.That(dbOperation, Is.Not.Null);
        Assert.That(dbOperation.IsDeleted, Is.EqualTo(false));
    }

    [Test]
    [TestCase(RegularTaskTimeTypes.EveryDay, null)]
    [TestCase(RegularTaskTimeTypes.EveryWeek, 1)]
    [TestCase(RegularTaskTimeTypes.EveryWeek, 2)]
    [TestCase(RegularTaskTimeTypes.EveryMonth, 1)]
    [TestCase(RegularTaskTimeTypes.EveryMonth, 15)]
    [TestCase(RegularTaskTimeTypes.EveryMonth, 31)]
    public async Task RunTimeTest(RegularTaskTimeTypes timeType, int? timeValue)
    {
        TestCategory category = _user.WithCategory();
        _dbClient.Save();

        TestRegularOperation operation = _user.WithRegularOperation();

        RegularOperationClient.SaveRequest request = new()
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
        if (timeType == RegularTaskTimeTypes.EveryDay)
        {
            runTime = DateTime.Now.Date.AddDays(1);
        }
        else if (timeType == RegularTaskTimeTypes.EveryWeek)
        {
            int daysToAdd = ((int)timeValue - (int)DateTime.Now.Date.AddDays(1).DayOfWeek + 7) % 7;
            runTime = DateTime.Now.Date.AddDays(1).AddDays(daysToAdd);
        }
        else if (timeType == RegularTaskTimeTypes.EveryMonth)
        {
            if (DateTime.Now.Day < timeValue)
            {
                runTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            }
            else
            {
                runTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1);
            }

            // если выбрали 31, то будет последний день месяца, 28 или 30 например
            var nextDt = runTime.AddDays(timeValue.Value - 1);
            if (runTime.Month < nextDt.Month)
            {
                runTime = runTime.AddMonths(1).AddDays(-1);
            }
            else
            {
                runTime = nextDt;
            }
        }
        else
        {
            throw new Exception();
        }

        int createdId = await _apiClient.RegularOperation.Create(request).IsSuccessWithContent();
        RegularOperation dbOperation = _dbClient.CreateApplicationDbContext().RegularOperations.Single(_user.Id, createdId);
        Assert.That(dbOperation.RunTime, Is.Not.Null);
        Assert.That(dbOperation.RunTime.Value, Is.EqualTo(runTime));
    }
}
