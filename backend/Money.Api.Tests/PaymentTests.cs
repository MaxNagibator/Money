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
    public async Task GetByDateFromTest()
    {
        TestCategory category = _user.WithCategory();

        TestPayment[] payments =
        [
            category.WithPayment().SetDate(DateTime.Now.Date.AddMonths(1)),
            category.WithPayment(),
        ];

        _dbClient.Save();

        PaymentClient.PaymentFilterDto filter = new()
        {
            DateFrom = DateTime.Now.Date.AddMonths(1),
        };

        PaymentClient.Payment[]? apiPayments = await _apiClient.Payment.Get(filter).IsSuccessWithContent();
        Assert.That(apiPayments, Is.Not.Null);
        Assert.That(apiPayments, Has.Length.EqualTo(1));
        Assert.That(apiPayments[0].Date, Is.GreaterThanOrEqualTo(DateTime.Now.Date));
    }

    [Test]
    public async Task GetByDateToTest()
    {
        TestCategory category = _user.WithCategory();

        TestPayment[] payments =
        [
            category.WithPayment().SetDate(DateTime.Now.Date.AddMonths(1)),
            category.WithPayment().SetDate(DateTime.Now.Date),
        ];

        _dbClient.Save();

        PaymentClient.PaymentFilterDto filter = new()
        {
            DateTo = DateTime.Now.Date.AddDays(1),
        };

        PaymentClient.Payment[]? apiPayments = await _apiClient.Payment.Get(filter).IsSuccessWithContent();
        Assert.That(apiPayments, Is.Not.Null);
        Assert.That(apiPayments, Has.Length.EqualTo(1));
        Assert.That(apiPayments[0].Date, Is.GreaterThanOrEqualTo(DateTime.Now.Date));
        Assert.That(apiPayments[0].Date, Is.LessThan(DateTime.Now.Date.AddDays(1)));
    }

    [Test]
    public async Task GetByDateRangeTest()
    {
        TestCategory category = _user.WithCategory();
        category.WithPayment().SetDate(DateTime.Now.Date.AddDays(-1));
        category.WithPayment().SetDate(DateTime.Now.Date);
        category.WithPayment().SetDate(DateTime.Now.Date.AddDays(1));
        _dbClient.Save();

        PaymentClient.PaymentFilterDto filter = new()
        {
            DateFrom = DateTime.Now.Date.AddDays(-1),
            DateTo = DateTime.Now.Date.AddDays(2),
        };

        PaymentClient.Payment[]? apiPayments = await _apiClient.Payment.Get(filter).IsSuccessWithContent();
        Assert.That(apiPayments, Is.Not.Null);
        Assert.That(apiPayments, Has.Length.EqualTo(3));
    }

    [Test]
    public async Task GetByCategoryIdsTest()
    {
        _user.WithCategory().WithPayment();
        TestCategory category = _user.WithCategory().WithPayment().Category;
        _dbClient.Save();

        PaymentClient.PaymentFilterDto filter = new()
        {
            CategoryIds = [category.Id],
        };

        PaymentClient.Payment[]? apiPayments = await _apiClient.Payment.Get(filter).IsSuccessWithContent();
        Assert.That(apiPayments, Is.Not.Null);
        Assert.That(apiPayments, Has.Length.EqualTo(1));
    }

    [Test]
    public async Task GetByCommentTest()
    {
        TestCategory category = _user.WithCategory();
        TestPayment payment = category.WithPayment().SetComment("ochen vazhniy platezh");
        category.WithPayment();
        _dbClient.Save();

        PaymentClient.PaymentFilterDto filter = new()
        {
            Comment = payment.Comment[..10],
        };

        PaymentClient.Payment[]? apiPayments = await _apiClient.Payment.Get(filter).IsSuccessWithContent();
        Assert.That(apiPayments, Is.Not.Null);
        Assert.That(apiPayments, Has.Length.EqualTo(1));
        Assert.That(apiPayments[0].Comment, Is.EqualTo(payment.Comment));
    }

    [Test]
    public async Task GetByPlaceTest()
    {
        TestPlace place = _user.WithPlace();
        TestCategory category = _user.WithCategory();
        TestPayment payment = category.WithPayment();
        payment.SetPlace(place);
        category.WithPayment();
        _dbClient.Save();

        PaymentClient.PaymentFilterDto filter = new()
        {
            Place = place.Name,
        };

        PaymentClient.Payment[]? apiPayments = await _apiClient.Payment.Get(filter).IsSuccessWithContent();
        Assert.That(apiPayments, Is.Not.Null);
        Assert.That(apiPayments, Has.Length.EqualTo(1));
        Assert.That(apiPayments[0].Place, Is.EqualTo(place.Name));
    }

    [Test]
    public async Task GetByMultipleCategoryIdsTest()
    {
        TestCategory category1 = _user.WithCategory();
        TestCategory category2 = _user.WithCategory();
        category1.WithPayment();
        category2.WithPayment();
        _dbClient.Save();

        PaymentClient.PaymentFilterDto filter = new()
        {
            CategoryIds = [category1.Id, category2.Id],
        };

        PaymentClient.Payment[]? apiPayments = await _apiClient.Payment.Get(filter).IsSuccessWithContent();
        Assert.That(apiPayments, Is.Not.Null);
        Assert.That(apiPayments, Has.Length.EqualTo(2));
    }

    [Test]
    public async Task GetNoPaymentsTest()
    {
        _dbClient.Save();

        PaymentClient.PaymentFilterDto filter = new()
        {
            DateFrom = DateTime.Now.AddDays(1),
        };

        PaymentClient.Payment[]? apiPayments = await _apiClient.Payment.Get(filter).IsSuccessWithContent();
        Assert.That(apiPayments, Is.Not.Null);
        Assert.That(apiPayments, Is.Empty);
    }
}
