using Money.Api.Tests.TestTools;
using Money.Api.Tests.TestTools.Entities;
using Money.ApiClient;
using Money.Data.Extensions;

namespace Money.Api.Tests.Debts;

public class DebtTests
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
        TestDebt[] debts =
        [
            _user.WithDebt(),
            _user.WithDebt(),
            _user.WithDebt(),
        ];

        _dbClient.Save();

        DebtClient.Debt[]? apiDebts = await _apiClient.Debt.Get(1).IsSuccessWithContent();
        Assert.That(apiDebts, Is.Not.Null);
        Assert.That(apiDebts.Count, Is.GreaterThanOrEqualTo(3));

        TestDebt[] testCategories = debts.ExceptBy(apiDebts.Select(x => x.Id), category => category.Id).ToArray();
        Assert.That(testCategories, Is.Not.Null);
        Assert.That(testCategories, Is.Empty);
    }

    [Test]
    public async Task GetByIdTest()
    {
        TestDebt debt = _user.WithDebt();
        _dbClient.Save();

        DebtClient.Debt? apiDebt = await _apiClient.Debt.GetById(debt.Id).IsSuccessWithContent();

        Assert.That(apiDebt, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(apiDebt.Id, Is.EqualTo(debt.Id));
            Assert.That(apiDebt.Comment, Is.EqualTo(debt.Comment));
            Assert.That(apiDebt.DebtUserName, Is.EqualTo(debt.DebtUserName));
            Assert.That(apiDebt.Date, Is.EqualTo(debt.Date));
            Assert.That(apiDebt.Sum, Is.EqualTo(debt.Sum));
        });
    }

    [Test]
    public async Task CreateTest()
    {
        _dbClient.Save();

        TestDebt debt = _user.WithDebt();

        DebtClient.SaveRequest request = new()
        {
            Comment = debt.Comment,
            DebtUserName = debt.DebtUserName,
            Sum = debt.Sum,
            Date = debt.Date,
            TypeId = (int)debt.Type,
        };

        int createdCategoryId = await _apiClient.Debt.Create(request).IsSuccessWithContent();
        Data.Entities.Debt? dbDebt = _dbClient.CreateApplicationDbContext().Debts.SingleOrDefault(_user.Id, createdCategoryId);

        Assert.That(dbDebt, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(dbDebt.Comment, Is.EqualTo(debt.Comment));
            Assert.That(dbDebt.Date, Is.EqualTo(debt.Date));
            Assert.That(dbDebt.Sum, Is.EqualTo(debt.Sum));
        });
    }
}
