using Money.ApiClient;
using Money.Data.Extensions;

namespace Money.Api.Tests.Operations;

public class OperationPlaceTests
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

    /// <summary>
    /// Создали операцию с оригинальным местом и место появилось.
    /// </summary>
    [Test]
    public async Task CreatePlaceTest()
    {
        var category = _user.WithCategory();
        _dbClient.Save();

        var request = new OperationClient.SaveRequest
        {
            CategoryId = category.Id,
            Sum = 1,
            Date = DateTime.Now,
            Place = "place1",
        };

        await _apiClient.Operation.Create(request).IsSuccess();

        var dbPlaces = _dbClient.CreateApplicationDbContext()
            .Places
            .IsUserEntity(_user.Id)
            .ToArray();

        Assert.That(dbPlaces, Has.Length.EqualTo(1));

        Assert.Multiple(() =>
        {
            Assert.That(dbPlaces[0].Name, Is.EqualTo(request.Place));
            Assert.That(dbPlaces[0].Id, Is.EqualTo(1));
            Assert.That(dbPlaces[0].IsDeleted, Is.False);
        });
    }

    /// <summary>
    /// Занулили место у единственной операции, место удалилось.
    /// </summary>
    [Test]
    [TestCase("")]
    [TestCase(null)]
    public async Task RemovePlaceAfterSetOperationZeroPlaceTest(string? updatedPlace)
    {
        var category = _user.WithCategory();
        _dbClient.Save();

        var request = new OperationClient.SaveRequest
        {
            CategoryId = category.Id,
            Sum = 1,
            Date = DateTime.Now,
            Place = "place1",
        };

        var operationId = await _apiClient.Operation.Create(request).IsSuccessWithContent();
        request.Place = updatedPlace;
        await _apiClient.Operation.Update(operationId, request).IsSuccess();

        var dbPlaces = _dbClient.CreateApplicationDbContext()
            .Places
            .Where(x => x.UserId == _user.Id)
            .ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(dbPlaces, Has.Length.EqualTo(1));
            Assert.That(dbPlaces[0].IsDeleted, Is.True);
        });
    }

    /// <summary>
    /// Две операции имеют одно место, и если место занулить у одной из операций, оно не удалится.
    /// </summary>
    [Test]
    public async Task DontRemovePlaceAfterSetOperationZeroPlaceTest()
    {
        var category = _user.WithCategory();
        _dbClient.Save();

        var request = new OperationClient.SaveRequest
        {
            CategoryId = category.Id,
            Sum = 1,
            Date = DateTime.Now,
            Place = "place1",
        };

        var operationId1 = await _apiClient.Operation.Create(request).IsSuccessWithContent();
        var operationId2 = await _apiClient.Operation.Create(request).IsSuccessWithContent();

        request.Place = null;
        await _apiClient.Operation.Update(operationId2, request).IsSuccess();

        var dbPlaces = _dbClient.CreateApplicationDbContext()
            .Places
            .Where(x => x.UserId == _user.Id)
            .ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(dbPlaces, Has.Length.EqualTo(1));
            Assert.That(dbPlaces[0].IsDeleted, Is.False);
        });
    }

    /// <summary>
    /// Две операции имеют одно место, после зануления места всех операций, место исчезло.
    /// </summary>
    [Test]
    public async Task RemovePlaceAfterSetAllOperationsZeroPlaceTest()
    {
        var category = _user.WithCategory();
        _dbClient.Save();

        var request = new OperationClient.SaveRequest
        {
            CategoryId = category.Id,
            Sum = 1,
            Date = DateTime.Now,
            Place = "place1",
        };

        var operationId1 = await _apiClient.Operation.Create(request).IsSuccessWithContent();
        var operationId2 = await _apiClient.Operation.Create(request).IsSuccessWithContent();

        request.Place = null;
        await _apiClient.Operation.Update(operationId1, request).IsSuccess();
        await _apiClient.Operation.Update(operationId2, request).IsSuccess();

        var dbPlaces = _dbClient.CreateApplicationDbContext()
            .Places
            .Where(x => x.UserId == _user.Id)
            .ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(dbPlaces, Has.Length.EqualTo(1));
            Assert.That(dbPlaces[0].IsDeleted, Is.True);
        });
    }

    /// <summary>
    /// Одна операция имеет уникальное место.
    /// После удаления операции, оно удалится.
    /// </summary>
    [Test]
    public async Task DeletePlaceAfterDeleteOperationTest()
    {
        var category = _user.WithCategory();
        _dbClient.Save();

        var request = new OperationClient.SaveRequest
        {
            CategoryId = category.Id,
            Sum = 1,
            Date = DateTime.Now,
            Place = "place1",
        };

        var operationId = await _apiClient.Operation.Create(request).IsSuccessWithContent();
        await _apiClient.Operation.Delete(operationId).IsSuccess();

        var dbPlaces = _dbClient.CreateApplicationDbContext()
            .Places
            .Where(x => x.UserId == _user.Id)
            .ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(dbPlaces, Has.Length.EqualTo(1));
            Assert.That(dbPlaces[0].IsDeleted, Is.True);
        });
    }

    /// <summary>
    /// Одна операция имеет уникальное место.
    /// После удаления операции, оно удалится.
    /// После восстановления операции должно восстановиться.
    /// </summary>
    [Test]
    public async Task RestorePlaceAfterRestoreOperationTest()
    {
        var category = _user.WithCategory();
        _dbClient.Save();

        var request = new OperationClient.SaveRequest
        {
            CategoryId = category.Id,
            Sum = 1,
            Date = DateTime.Now,
            Place = "place1",
        };

        var operationId = await _apiClient.Operation.Create(request).IsSuccessWithContent();
        await _apiClient.Operation.Delete(operationId).IsSuccess();
        await _apiClient.Operation.Restore(operationId).IsSuccess();

        var dbPlaces = _dbClient.CreateApplicationDbContext()
            .Places
            .Where(x => x.UserId == _user.Id)
            .ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(dbPlaces, Has.Length.EqualTo(1));
            Assert.That(dbPlaces[0].IsDeleted, Is.False);
        });
    }

    /// <summary>
    /// Одна операция имеет уникальное место.
    /// После удаления операции, оно удалится.
    /// После восстановления операции должно
    /// восстановиться.
    /// </summary>
    [Test]
    public async Task RestorePlaceAfterCreateOperationWithDeletedPlaceTest()
    {
        var category = _user.WithCategory();
        _dbClient.Save();

        var request = new OperationClient.SaveRequest
        {
            CategoryId = category.Id,
            Sum = 1,
            Date = DateTime.Now,
            Place = "place1",
        };

        var operationId1 = await _apiClient.Operation.Create(request).IsSuccessWithContent();
        await _apiClient.Operation.Delete(operationId1).IsSuccess();
        await _apiClient.Operation.Create(request).IsSuccessWithContent();

        var dbPlaces = _dbClient.CreateApplicationDbContext()
            .Places
            .Where(x => x.UserId == _user.Id)
            .ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(dbPlaces, Has.Length.EqualTo(1));
            Assert.That(dbPlaces[0].IsDeleted, Is.False);
        });
    }

    /// <summary>
    /// Создадим три плейса и проверим параметры offset и count.
    /// </summary>
    [Test]
    public async Task GetPlacesOffsetAndCountTest()
    {
        _user.WithCategory();
        var place = _user.WithPlace();
        _user.WithPlace();
        _user.WithPlace();
        _dbClient.Save();

        var apiPlaces = await _apiClient.Operation.GetPlaces(0, 1, place.Name.Substring(0, 1)).IsSuccessWithContent();
        Assert.That(apiPlaces, Is.Not.Null);
        Assert.That(apiPlaces, Has.Length.EqualTo(1));

        apiPlaces = await _apiClient.Operation.GetPlaces(1, 10, place.Name.Substring(0, 1)).IsSuccessWithContent();
        Assert.That(apiPlaces, Is.Not.Null);
        Assert.That(apiPlaces, Has.Length.EqualTo(2));

        apiPlaces = await _apiClient.Operation.GetPlaces(2, 10, place.Name.Substring(0, 1)).IsSuccessWithContent();
        Assert.That(apiPlaces, Is.Not.Null);
        Assert.That(apiPlaces, Has.Length.EqualTo(1));
    }

    /// <summary>
    /// Создадим три плейса и проверим параметры offset и count.
    /// </summary>
    [Test]
    public async Task GetPlacesWithEmptySearchTest()
    {
        _user.WithCategory();
        _user.WithPlace();
        _dbClient.Save();

        var apiPlaces = await _apiClient.Operation.GetPlaces(0, 1).IsSuccessWithContent();
        Assert.That(apiPlaces, Is.Not.Null);
        Assert.That(apiPlaces, Has.Length.EqualTo(1));

        apiPlaces = await _apiClient.Operation.GetPlaces(0, 1, "").IsSuccessWithContent();
        Assert.That(apiPlaces, Is.Not.Null);
        Assert.That(apiPlaces, Has.Length.EqualTo(1));
    }

    /// <summary>
    /// Если фильтр пустой, на первой позиции будем последнее используемое место.
    /// </summary>
    [Test]
    public async Task GetPlacesOrderByLastUsedDateTest()
    {
        _user.WithCategory();
        _user.WithPlace().SetLastUsedDate(DateTime.Now.AddMinutes(-1));
        var place3 = _user.WithPlace().SetLastUsedDate(DateTime.Now.AddMinutes(-2));
        var place1 = _user.WithPlace().SetLastUsedDate(DateTime.Now);
        _dbClient.Save();

        var apiPlaces = await _apiClient.Operation.GetPlaces(0, 100).IsSuccessWithContent();
        Assert.That(apiPlaces, Is.Not.Null);
        Assert.That(apiPlaces, Has.Length.EqualTo(3));

        Assert.Multiple(() =>
        {
            Assert.That(apiPlaces[0], Is.EqualTo(place1.Name));
            Assert.That(apiPlaces[2], Is.EqualTo(place3.Name));
        });
    }
}
