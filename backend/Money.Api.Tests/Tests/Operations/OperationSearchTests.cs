using Money.ApiClient;

namespace Money.Api.Tests.Tests.Operations;

public class OperationSearchTests
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
    public async Task CombinedSearchCaseInsensitiveTest()
    {
        // Arrange
        var category = _user.WithCategory();
        var place1 = _user.WithPlace().SetName("TEST Store");
        var place2 = _user.WithPlace().SetName("Other Place");

        var operation1 = category.WithOperation()
            .SetComment("GROCERY shopping")
            .SetPlace(place1);

        category.WithOperation()
            .SetComment("grocery shopping")
            .SetPlace(place2);

        category.WithOperation()
            .SetComment("other activity")
            .SetPlace(place1);

        _dbClient.Save();

        var filter = new OperationsClient.OperationFilterDto
        {
            Comment = "grocery",
            Place = "test",
        };

        // Act
        var apiOperations = await _apiClient.Operations.Get(filter).IsSuccessWithContent();

        // Assert
        Assert.That(apiOperations, Is.Not.Null);
        Assert.That(apiOperations, Has.Length.EqualTo(1));

        Assert.Multiple(() =>
        {
            Assert.That(apiOperations[0].Comment, Is.EqualTo(operation1.Comment));
            Assert.That(apiOperations[0].Place, Is.EqualTo(place1.Name));
        });
    }

    [Test]
    [TestCase("test comment", "TEST")]
    [TestCase("Test Comment", "test")]
    [TestCase("UPPER CASE", "upper")]
    [TestCase("Mixed CaSe", "mixed case")]
    public async Task CommentSearchCaseInsensitiveTest(string originalComment, string searchTerm)
    {
        // Arrange
        var category = _user.WithCategory();
        category.WithOperation().SetComment(originalComment);
        _dbClient.Save();

        var filter = new OperationsClient.OperationFilterDto
        {
            Comment = searchTerm,
        };

        // Act
        var apiOperations = await _apiClient.Operations.Get(filter).IsSuccessWithContent();

        // Assert
        Assert.That(apiOperations, Is.Not.Null);
        Assert.That(apiOperations, Has.Length.EqualTo(1));
        Assert.That(apiOperations[0].Comment, Is.EqualTo(originalComment));
    }

    [Test]
    public async Task EmptySearchTermsTest()
    {
        // Arrange
        var category = _user.WithCategory();
        category.WithOperation().SetComment("Test Comment");
        _dbClient.Save();

        var filter = new OperationsClient.OperationFilterDto
        {
            Comment = "",
            Place = "",
        };

        // Act
        var apiOperations = await _apiClient.Operations.Get(filter).IsSuccessWithContent();

        // Assert
        Assert.That(apiOperations, Is.Not.Null);
        Assert.That(apiOperations, Has.Length.EqualTo(1));
    }

    [Test]
    [TestCase("Test Place", "TEST")]
    [TestCase("UPPER PLACE", "upper")]
    [TestCase("Mixed CaSe Place", "mixed case")]
    public async Task GetPlacesCaseInsensitiveTest(string originalPlace, string searchTerm)
    {
        // Arrange
        _user.WithPlace().SetName(originalPlace);
        _dbClient.Save();

        // Act
        var apiPlaces = await _apiClient.Operations.GetPlaces(0, 10, searchTerm).IsSuccessWithContent();

        // Assert
        Assert.That(apiPlaces, Is.Not.Null);
        Assert.That(apiPlaces, Has.Length.EqualTo(1));
        Assert.That(apiPlaces[0], Is.EqualTo(originalPlace));
    }

    [Test]
    public async Task PartialMatchCaseInsensitiveTest()
    {
        // Arrange
        var category = _user.WithCategory();
        var operation1 = category.WithOperation().SetComment("This is a TEST comment");
        var operation2 = category.WithOperation().SetComment("Another test COMMENT here");
        var operation3 = category.WithOperation().SetComment("No match here");
        _dbClient.Save();

        var filter = new OperationsClient.OperationFilterDto
        {
            Comment = "test comment",
        };

        // Act
        var apiOperations = await _apiClient.Operations.Get(filter).IsSuccessWithContent();

        // Assert
        Assert.That(apiOperations, Is.Not.Null);
        Assert.That(apiOperations, Has.Length.EqualTo(2));

        var comments = apiOperations.Select(x => x.Comment).ToArray();
        Assert.That(comments, Contains.Item(operation1.Comment));
        Assert.That(comments, Contains.Item(operation2.Comment));
        Assert.That(comments, Does.Not.Contain(operation3.Comment));
    }

    [Test]
    [TestCase("Test Place", "TEST")]
    [TestCase("UPPER PLACE", "upper")]
    [TestCase("Mixed CaSe Place", "mixed case")]
    public async Task PlaceSearchCaseInsensitiveTest(string originalPlace, string searchTerm)
    {
        // Arrange
        var category = _user.WithCategory();
        var place = _user.WithPlace().SetName(originalPlace);
        category.WithOperation().SetPlace(place);
        _dbClient.Save();

        var filter = new OperationsClient.OperationFilterDto
        {
            Place = searchTerm,
        };

        // Act
        var apiOperations = await _apiClient.Operations.Get(filter).IsSuccessWithContent();

        // Assert
        Assert.That(apiOperations, Is.Not.Null);
        Assert.That(apiOperations, Has.Length.EqualTo(1));
        Assert.That(apiOperations[0].Place, Is.EqualTo(originalPlace));
    }
}
