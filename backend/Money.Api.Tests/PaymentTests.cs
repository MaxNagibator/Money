using Money.Api.Tests.TestTools;
using Money.Api.Tests.TestTools.Entities;
using Money.ApiClient;

namespace Money.Api.Tests;

public class PaymentTests
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
        // todo айди платежа начинается не с единицы
        TestCategory category = _user.WithCategory();

        TestPayment[] payments =
        [
            category.WithPayment(),
            category.WithPayment(),
            category.WithPayment(),
        ];

        _dbClient.Save();

        PaymentClient.Payment[]? apiPayments = await _apiClient.Payment.Get().IsSuccessWithContent();
        Assert.That(apiPayments, Is.Not.Null);
        Assert.That(apiPayments, Has.Length.GreaterThanOrEqualTo(payments.Length));

        TestPayment[] testCategories = payments.ExceptBy(apiPayments.Select(x => x.Id), payment => payment.Id).ToArray();
        Assert.That(testCategories, Is.Not.Null);
        Assert.That(testCategories, Is.Empty);
    }

    [Test]
    public async Task GetByDateTest()
    {
        TestCategory category = _user.WithCategory();
        TestPayment[] payments =
        [
            category.WithPayment().SetDate(DateTime.Now.AddMonths(1)),
            category.WithPayment(),
        ];

        _dbClient.Save();

        var filter = new PaymentClient.PaymentFilterDto
        {
            DateFrom = DateTime.Now.Date,
            DateTo = DateTime.Now.Date.AddDays(1),
        };
        PaymentClient.Payment[]? apiPayments = await _apiClient.Payment.Get(filter).IsSuccessWithContent();
        Assert.That(apiPayments, Is.Not.Null);
        Assert.That(apiPayments.Length, Is.EqualTo(1));
    }

    [Test]
    public async Task GetByCategoryIdsTest()
    {
        _user.WithCategory().WithPayment();
        var category = _user.WithCategory().WithPayment().Category;
        _dbClient.Save();

        var filter = new PaymentClient.PaymentFilterDto
        {
            CategoryIds = new List<int> { category.Id },
        };
        PaymentClient.Payment[]? apiPayments = await _apiClient.Payment.Get(filter).IsSuccessWithContent();
        Assert.That(apiPayments, Is.Not.Null);
        Assert.That(apiPayments.Length, Is.EqualTo(1));
    }

    [Test]
    public async Task GetByCommentTest()
    {
        var category = _user.WithCategory();
        var payment = category.WithPayment().SetComment("ochen vazhniy platezh");
        category.WithPayment();
        _dbClient.Save();

        var filter = new PaymentClient.PaymentFilterDto
        {
            Comment = payment.Comment.Substring(0, 10),
        };
        PaymentClient.Payment[]? apiPayments = await _apiClient.Payment.Get(filter).IsSuccessWithContent();
        Assert.That(apiPayments, Is.Not.Null);
        Assert.That(apiPayments.Length, Is.EqualTo(1));
    }

    [Test]
    public async Task GetByPlaceTest()
    {
        var place = _user.WithPlace();
        var category = _user.WithCategory();
        var payment = category.WithPayment();
        payment.SetPlace(place);
        category.WithPayment();
        _dbClient.Save();

        var filter = new PaymentClient.PaymentFilterDto
        {
            Place = place.Name,
        };
        PaymentClient.Payment[]? apiPayments = await _apiClient.Payment.Get(filter).IsSuccessWithContent();
        Assert.That(apiPayments, Is.Not.Null);
        Assert.That(apiPayments.Length, Is.EqualTo(1));
    }
}
