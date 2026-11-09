using Money.ApiClient;
using Money.Business.Enums;
using System.Net;

namespace Money.Api.Tests.Tests.Operations;

public class OperationInferCategoryTests
{
    private DatabaseClient _dbClient = null!;
    private TestUser _user = null!;
    private MoneyClient _apiClient = null!;

    [SetUp]
    public void SetUp()
    {
        _dbClient = Integration.GetDatabaseClient();
        _user = _dbClient.WithUser();
        _apiClient = new(Integration.GetHttpClient(), Console.WriteLine);
        _apiClient.SetUser(_user);
    }

    [Test]
    public async Task AllSameCategoryReturnsCategoryTest()
    {
        var category = _user.WithCategory().SetOperationType(OperationTypes.Costs);
        var place = _user.WithPlace();

        for (var i = 0; i < 5; i++)
        {
            category.WithOperation().SetPlace(place).SetDate(DateTime.Today.AddDays(-i));
        }

        _dbClient.Save();

        var response = await _apiClient.Operations.InferCategoryByPlace(place.Name, (int)OperationTypes.Costs).IsSuccess();

        Assert.Multiple(() =>
        {
            Assert.That(response.Code, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content, Is.Not.Null);
        });
        Assert.That(response.Content!.CategoryId, Is.EqualTo(category.Id));
    }

    [Test]
    public async Task DeletedCategoryExcludedTest()
    {
        var deletedCategory = _user.WithCategory().SetOperationType(OperationTypes.Costs);
        deletedCategory.IsDeleted = true;

        var place = _user.WithPlace();

        for (var i = 0; i < 5; i++)
        {
            deletedCategory.WithOperation().SetPlace(place).SetDate(DateTime.Today.AddDays(-i));
        }

        _dbClient.Save();

        var response = await _apiClient.Operations.InferCategoryByPlace(place.Name, (int)OperationTypes.Costs).IsSuccess();

        Assert.That(response.Code, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task DeletedOperationExcludedTest()
    {
        var category1 = _user.WithCategory().SetOperationType(OperationTypes.Costs);
        var category2 = _user.WithCategory().SetOperationType(OperationTypes.Costs);
        var place = _user.WithPlace();

        category2.WithOperation().SetPlace(place).SetDate(DateTime.Today.AddDays(-10)).SetIsDeleted();
        category1.WithOperation().SetPlace(place).SetDate(DateTime.Today);
        category1.WithOperation().SetPlace(place).SetDate(DateTime.Today.AddDays(-1));

        _dbClient.Save();

        var response = await _apiClient.Operations.InferCategoryByPlace(place.Name, (int)OperationTypes.Costs).IsSuccess();

        Assert.Multiple(() =>
        {
            Assert.That(response.Code, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content!.CategoryId, Is.EqualTo(category1.Id));
        });
    }

    [Test]
    public async Task DifferentUserIgnoredTest()
    {
        var category = _user.WithCategory().SetOperationType(OperationTypes.Costs);
        var place = _user.WithPlace();
        category.WithOperation().SetPlace(place).SetDate(DateTime.Today);
        category.WithOperation().SetPlace(place).SetDate(DateTime.Today.AddDays(-1));

        var otherUser = _dbClient.WithUser();
        var otherCategory = otherUser.WithCategory().SetOperationType(OperationTypes.Costs);
        var otherPlace = otherUser.WithPlace().SetName(place.Name);
        otherCategory.WithOperation().SetPlace(otherPlace).SetDate(DateTime.Today);

        _dbClient.Save();

        var response = await _apiClient.Operations.InferCategoryByPlace(place.Name, (int)OperationTypes.Costs).IsSuccess();

        Assert.Multiple(() =>
        {
            Assert.That(response.Code, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content!.CategoryId, Is.EqualTo(category.Id));
        });
    }

    [Test]
    public async Task ExactlyTwoSameCategoryReturnsCategoryTest()
    {
        var category = _user.WithCategory().SetOperationType(OperationTypes.Costs);
        var place = _user.WithPlace();

        category.WithOperation().SetPlace(place).SetDate(DateTime.Today);
        category.WithOperation().SetPlace(place).SetDate(DateTime.Today.AddDays(-1));

        _dbClient.Save();

        var response = await _apiClient.Operations.InferCategoryByPlace(place.Name, (int)OperationTypes.Costs).IsSuccess();

        Assert.Multiple(() =>
        {
            Assert.That(response.Code, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content!.CategoryId, Is.EqualTo(category.Id));
        });
    }

    [Test]
    public async Task FourSameOneDifferentReturnsNoContentTest()
    {
        var category1 = _user.WithCategory().SetOperationType(OperationTypes.Costs);
        var category2 = _user.WithCategory().SetOperationType(OperationTypes.Costs);
        var place = _user.WithPlace();

        for (var i = 0; i < 4; i++)
        {
            category1.WithOperation().SetPlace(place).SetDate(DateTime.Today.AddDays(-i - 1));
        }

        category2.WithOperation().SetPlace(place).SetDate(DateTime.Today);

        _dbClient.Save();

        var response = await _apiClient.Operations.InferCategoryByPlace(place.Name, (int)OperationTypes.Costs).IsSuccess();

        Assert.That(response.Code, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task MismatchedOperationTypeFilteredTest()
    {
        var expenseCategory = _user.WithCategory().SetOperationType(OperationTypes.Costs);
        var incomeCategory = _user.WithCategory().SetOperationType(OperationTypes.Income);
        var place = _user.WithPlace();

        incomeCategory.WithOperation().SetPlace(place).SetDate(DateTime.Today);
        expenseCategory.WithOperation().SetPlace(place).SetDate(DateTime.Today.AddDays(-1));

        _dbClient.Save();

        var response = await _apiClient.Operations.InferCategoryByPlace(place.Name, (int)OperationTypes.Costs).IsSuccess();

        Assert.That(response.Code, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task PlaceNotFoundReturnsNoContentTest()
    {
        _user.WithCategory().SetOperationType(OperationTypes.Costs);
        _dbClient.Save();

        var response = await _apiClient.Operations.InferCategoryByPlace("never-used-place", (int)OperationTypes.Costs).IsSuccess();

        Assert.That(response.Code, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task RespectsCountParameterOlderInconsistencyIgnoredTest()
    {
        var consistentCategory = _user.WithCategory().SetOperationType(OperationTypes.Costs);
        var noiseCategory = _user.WithCategory().SetOperationType(OperationTypes.Costs);
        var place = _user.WithPlace();

        for (var i = 0; i < 5; i++)
        {
            consistentCategory.WithOperation().SetPlace(place).SetDate(DateTime.Today.AddDays(-i));
        }

        for (var i = 0; i < 5; i++)
        {
            noiseCategory.WithOperation().SetPlace(place).SetDate(DateTime.Today.AddDays(-10 - i));
        }

        _dbClient.Save();

        var response = await _apiClient.Operations.InferCategoryByPlace(place.Name, (int)OperationTypes.Costs).IsSuccess();

        Assert.Multiple(() =>
        {
            Assert.That(response.Code, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content!.CategoryId, Is.EqualTo(consistentCategory.Id));
        });
    }

    [Test]
    public async Task SingleOperationReturnsNoContentTest()
    {
        var category = _user.WithCategory().SetOperationType(OperationTypes.Costs);
        var place = _user.WithPlace();
        category.WithOperation().SetPlace(place).SetDate(DateTime.Today);

        _dbClient.Save();

        var response = await _apiClient.Operations.InferCategoryByPlace(place.Name, (int)OperationTypes.Costs).IsSuccess();

        Assert.That(response.Code, Is.EqualTo(HttpStatusCode.NoContent));
    }
}
