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
        TestCar[] models =
        [
            _user.WithCar(),
            _user.WithCar(),
            _user.WithCar(),
        ];

        _dbClient.Save();

        var apiModels = await _apiClient.Cars.Get().IsSuccessWithContent();
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
        var model = _user.WithCar();
        _dbClient.Save();

        var apiModel = await _apiClient.Cars.GetById(model.Id).IsSuccessWithContent();

        Assert.That(apiModel, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(apiModel.Id, Is.EqualTo(model.Id));
            Assert.That(apiModel.Name, Is.EqualTo(model.Name));
        });
    }

    [Test]
    public async Task CreateTest()
    {
        _dbClient.Save();

        var model = _user.WithCar();

        var request = new CarsClient.SaveRequest
        {
            Name = model.Name,
        };

        var createdId = await _apiClient.Cars.Create(request).IsSuccessWithContent();
        var entity = await _dbClient.CreateApplicationDbContext().Cars.FirstOrDefaultAsync(_user.Id, createdId);

        Assert.That(entity, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(entity.Name, Is.EqualTo(request.Name));
        });
    }

    [Test]
    public async Task UpdateTest()
    {
        var model = _user.WithCar();
        _dbClient.Save();

        var request = new CarsClient.SaveRequest
        {
            Name = model.Name,
        };

        await _apiClient.Cars.Update(model.Id, request).IsSuccess();
        var entity = await _dbClient.CreateApplicationDbContext().Cars.FirstOrDefaultAsync(_user.Id, model.Id);

        Assert.That(entity, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(entity.Name, Is.EqualTo(request.Name));
        });
    }

    [Test]
    public async Task DeleteTest()
    {
        var model = _user.WithCar();
        _dbClient.Save();

        await _apiClient.Cars.Delete(model.Id).IsSuccess();

        await using var context = _dbClient.CreateApplicationDbContext();

        var entity = await context.Cars
            .FirstOrDefaultAsync(_user.Id, model.Id);

        Assert.That(entity, Is.Null);

        entity = await context.Cars
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(_user.Id, model.Id);

        Assert.That(entity, Is.Not.Null);
        Assert.That(entity.IsDeleted, Is.True);
    }

    [Test]
    public async Task RestoreTest()
    {
        var model = _user.WithCar().SetIsDeleted();
        _dbClient.Save();

        await _apiClient.Cars.Restore(model.Id).IsSuccess();

        var entity = await _dbClient.CreateApplicationDbContext()
            .Cars
            .FirstOrDefaultAsync(_user.Id, model.Id);

        Assert.That(entity, Is.Not.Null);
        Assert.That(entity.IsDeleted, Is.False);
    }

    [Test]
    public async Task RestoreFailWhenNotDeletedTest()
    {
        var model = _user.WithCar();
        _dbClient.Save();

        await _apiClient.Cars.Restore(model.Id).IsBadRequest();
    }

    [Test]
    public async Task RestoreFailWhenNotExistTest()
    {
        _dbClient.Save();

        await _apiClient.Cars.Restore(-1).IsNotFound();
    }
}
