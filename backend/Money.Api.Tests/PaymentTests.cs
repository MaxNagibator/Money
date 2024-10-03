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
        // todo добавить дату в создание тестового платежа.
        TestCategory category = _user.WithCategory();

        TestPayment[] payments =
        [
            category.WithPayment(),
            category.WithPayment(),
            category.WithPayment(),
        ];

        _dbClient.Save();

        PaymentClient.Category[]? apiPayments = await _apiClient.Payment.Get().IsSuccessWithContent();
        Assert.That(apiPayments, Is.Not.Null);
        Assert.That(apiPayments, Has.Length.GreaterThanOrEqualTo(payments.Length));

        // TestPayment[] testCategories = payments.ExceptBy(apiPayments.Select(x => x.Id), payment => payment.Id).ToArray();
        // Assert.That(testCategories, Is.Not.Null);
        // Assert.That(testCategories, Is.Empty);
    }
}
