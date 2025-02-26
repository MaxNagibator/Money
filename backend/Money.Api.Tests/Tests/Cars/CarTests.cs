using Microsoft.EntityFrameworkCore;
using Money.ApiClient;
using Money.Data.Extensions;

namespace Money.Api.Tests.Cars;

public class CarTests
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
        TestCar[] cars =
        [
            _user.WithCar(),
            _user.WithCar(),
            _user.WithCar(),
        ];

        _dbClient.Save();

        var apiCars = await _apiClient.Cars.Get().IsSuccessWithContent();
        Assert.That(apiCars, Is.Not.Null);
        Assert.That(apiCars.Count, Is.GreaterThanOrEqualTo(3));
        Assert.That(apiCars[0].Id, Is.EqualTo(cars[0].Id));

        var testCars = cars.ExceptBy(apiCars.Select(x => x.Id), category => category.Id).ToArray();
        Assert.That(testCars, Is.Not.Null);
        Assert.That(testCars, Is.Empty);
    }

    [Test]
    public async Task GetByIdTest()
    {
        var category = _user.WithCar();
        _dbClient.Save();

        var apiCar = await _apiClient.Cars.GetById(category.Id).IsSuccessWithContent();

        Assert.That(apiCar, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(apiCar.Id, Is.EqualTo(category.Id));
            Assert.That(apiCar.Name, Is.EqualTo(category.Name));
        });
    }

    [Test]
    public async Task CreateTest()
    {
        _dbClient.Save();

        var category = _user.WithCar();

        var request = new CarsClient.SaveRequest
        {
            Name = category.Name,
        };

        var createdCarId = await _apiClient.Cars.Create(request).IsSuccessWithContent();
        var dbCar = await _dbClient.CreateApplicationDbContext().Cars.FirstOrDefaultAsync(_user.Id, createdCarId);

        Assert.That(dbCar, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(dbCar.Name, Is.EqualTo(request.Name));
        });
    }

    [Test]
    public async Task UpdateTest()
    {
        var category = _user.WithCar();
        _dbClient.Save();

        var request = new CarsClient.SaveRequest
        {
            Name = category.Name,
        };

        await _apiClient.Cars.Update(category.Id, request).IsSuccess();
        var dbCar = await _dbClient.CreateApplicationDbContext().Cars.FirstOrDefaultAsync(_user.Id, category.Id);

        Assert.That(dbCar, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(dbCar.Name, Is.EqualTo(request.Name));
        });
    }

    [Test]
    public async Task DeleteTest()
    {
        var category = _user.WithCar();
        _dbClient.Save();

        await _apiClient.Cars.Delete(category.Id).IsSuccess();

        await using var context = _dbClient.CreateApplicationDbContext();

        var dbCar = await context.Cars
            .FirstOrDefaultAsync(_user.Id, category.Id);

        Assert.That(dbCar, Is.Null);

        dbCar = await context.Cars
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(_user.Id, category.Id);

        Assert.That(dbCar, Is.Not.Null);
        Assert.That(dbCar.IsDeleted, Is.EqualTo(true));
    }

    [Test]
    public async Task RestoreTest()
    {
        var category = _user.WithCar().SetIsDeleted();
        _dbClient.Save();

        await _apiClient.Cars.Restore(category.Id).IsSuccess();

        await using var context = _dbClient.CreateApplicationDbContext();

        var dbCar = await context.Cars.FirstOrDefaultAsync(_user.Id, category.Id);
        Assert.That(dbCar, Is.Not.Null);
        Assert.That(dbCar.IsDeleted, Is.EqualTo(false));
    }

    [Test]
    public async Task RestoreFailWhenNotDeletedTest()
    {
        var category = _user.WithCar();
        _dbClient.Save();

        await _apiClient.Cars.Restore(category.Id).IsBadRequest();
    }

    [Test]
    public async Task RestoreFailWhenNotExistTest()
    {
        _dbClient.Save();

        await _apiClient.Cars.Restore(-1).IsNotFound();
    }
}
