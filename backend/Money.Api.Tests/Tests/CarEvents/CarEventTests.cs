using Microsoft.EntityFrameworkCore;
using Money.ApiClient;
using Money.Data.Extensions;

namespace Money.Api.Tests.CarEvents;

public class CarEventTests
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
        var car = _user.WithCar();

        TestCarEvent[] models =
        [
            car.WithEvent(),
            car.WithEvent(),
            car.WithEvent(),
        ];

        _dbClient.Save();

        var apiModels = await _apiClient.CarEvents.Get(car.Id).IsSuccessWithContent();
        Assert.That(apiModels, Is.Not.Null);
        Assert.That(apiModels.Count, Is.GreaterThanOrEqualTo(3));
        Assert.That(apiModels[0].Id, Is.EqualTo(models[0].Id));

        var exceptModels = models.ExceptBy(apiModels.Select(x => x.Id), model => model.Id).ToArray();
        Assert.That(exceptModels, Is.Not.Null);
        Assert.That(exceptModels, Is.Empty);
    }

    [Test]
    public async Task GetByIdTest()
    {
        var model = _user.WithCar().WithEvent();
        _dbClient.Save();

        var apiModel = await _apiClient.CarEvents.GetById(model.Id).IsSuccessWithContent();

        Assert.That(apiModel, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(apiModel.Id, Is.EqualTo(model.Id));
            Assert.That(apiModel.Title, Is.EqualTo(model.Title));
            Assert.That(apiModel.TypeId, Is.EqualTo(model.TypeId));
            Assert.That(apiModel.Comment, Is.EqualTo(model.Comment));
            Assert.That(apiModel.Mileage, Is.EqualTo(model.Mileage));
            Assert.That(apiModel.Date, Is.EqualTo(model.Date));
        });
    }

    [Test]
    public async Task CreateTest()
    {
        var model = _user.WithCar().WithEvent();
        _dbClient.Save();

        var request = new CarEventsClient.SaveRequest
        {
            CarId = model.Car.Id,
            Title = model.Title,
            TypeId = model.TypeId,
            Comment = model.Comment,
            Mileage = model.Mileage,
            Date = model.Date,
        };

        var createdId = await _apiClient.CarEvents.Create(request).IsSuccessWithContent();
        var entity = await _dbClient.CreateApplicationDbContext().CarEvents.FirstOrDefaultAsync(_user.Id, createdId);

        Assert.That(entity, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(entity.Title, Is.EqualTo(request.Title));
            Assert.That(entity.TypeId, Is.EqualTo(request.TypeId));
            Assert.That(entity.Comment, Is.EqualTo(request.Comment));
            Assert.That(entity.Mileage, Is.EqualTo(request.Mileage));
            Assert.That(entity.Date, Is.EqualTo(request.Date));
        });
    }

    [Test]
    public async Task UpdateTest()
    {
        var model = _user.WithCar().WithEvent();
        _dbClient.Save();

        var request = new CarEventsClient.SaveRequest
        {
            CarId = model.Car.Id,
            Title = model.Title,
            TypeId = model.TypeId,
            Comment = model.Comment,
            Mileage = model.Mileage,
            Date = model.Date,
        };

        await _apiClient.CarEvents.Update(model.Id, request).IsSuccess();
        var entity = await _dbClient.CreateApplicationDbContext().CarEvents.FirstOrDefaultAsync(_user.Id, model.Id);

        Assert.That(entity, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(entity.Title, Is.EqualTo(request.Title));
            Assert.That(entity.TypeId, Is.EqualTo(request.TypeId));
            Assert.That(entity.Comment, Is.EqualTo(request.Comment));
            Assert.That(entity.Mileage, Is.EqualTo(request.Mileage));
            Assert.That(entity.Date, Is.EqualTo(request.Date));
        });
    }

    [Test]
    public async Task DeleteTest()
    {
        var model = _user.WithCar().WithEvent();
        _dbClient.Save();

        await _apiClient.CarEvents.Delete(model.Id).IsSuccess();

        await using var context = _dbClient.CreateApplicationDbContext();

        var entity = await context.CarEvents
            .FirstOrDefaultAsync(_user.Id, model.Id);

        Assert.That(entity, Is.Null);

        entity = await context.CarEvents
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(_user.Id, model.Id);

        Assert.That(entity, Is.Not.Null);
        Assert.That(entity.IsDeleted, Is.True);
    }

    [Test]
    public async Task RestoreTest()
    {
        var model = _user.WithCar().WithEvent().SetIsDeleted();
        _dbClient.Save();

        await _apiClient.CarEvents.Restore(model.Id).IsSuccess();

        var entity = await _dbClient.CreateApplicationDbContext()
            .CarEvents
            .FirstOrDefaultAsync(_user.Id, model.Id);

        Assert.That(entity, Is.Not.Null);
        Assert.That(entity.IsDeleted, Is.False);
    }

    [Test]
    public async Task RestoreFailWhenNotDeletedTest()
    {
        var model = _user.WithCar().WithEvent();
        _dbClient.Save();

        await _apiClient.CarEvents.Restore(model.Id).IsBadRequest();
    }

    [Test]
    public async Task RestoreFailWhenNotExistTest()
    {
        _dbClient.Save();

        await _apiClient.CarEvents.Restore(-1).IsNotFound();
    }

    [Test]
    [TestCaseSource(nameof(GetInvalidRequests))]
    public async Task ValidationTestCreate(CarEventsClient.SaveRequest request)
    {
        _user.WithCar();
        _dbClient.Save();

        var result = await _apiClient.CarEvents.Create(request);
        Assert.That(result.IsSuccessStatusCode, Is.False);
    }

    [Test]
    [TestCaseSource(nameof(GetInvalidRequests))]
    public async Task ValidationTestUpdate(CarEventsClient.SaveRequest request)
    {
        var car = _user.WithCar();
        var existingEvent = car.WithEvent();
        _dbClient.Save();

        var result = await _apiClient.CarEvents.Update(existingEvent.Id, request);
        Assert.That(result.IsSuccessStatusCode, Is.False);
    }

    private static IEnumerable<CarEventsClient.SaveRequest> GetInvalidRequests()
    {
        var baseRequest = new CarEventsClient.SaveRequest
        {
            Title = "Кибитка",
            TypeId = 1,
            Mileage = 1000,
            Date = DateTime.Now,
        };

        yield return new()
        {
            CarId = baseRequest.CarId,
            Title = baseRequest.Title,
            TypeId = 999,
            Mileage = baseRequest.Mileage,
            Date = baseRequest.Date,
        };

        yield return new()
        {
            CarId = baseRequest.CarId,
            Title = new('a', 1001),
            TypeId = baseRequest.TypeId,
            Mileage = baseRequest.Mileage,
            Date = baseRequest.Date,
        };

        yield return new()
        {
            CarId = baseRequest.CarId,
            Title = baseRequest.Title,
            TypeId = baseRequest.TypeId,
            Mileage = -1,
            Date = baseRequest.Date,
        };
    }
}
