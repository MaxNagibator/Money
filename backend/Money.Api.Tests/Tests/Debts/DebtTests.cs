using Microsoft.EntityFrameworkCore;
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
        _apiClient = new(Integration.GetHttpClient(), Console.WriteLine);
        _apiClient.SetUser(_user);
    }

    [Test]
    public async Task GetTest()
    {
        var debts = new[]
        {
            _user.WithDebt(),
            _user.WithDebt(),
            _user.WithDebt(),
        };

        _dbClient.Save();

        var apiDebts = await _apiClient.Debt.Get(1).IsSuccessWithContent();
        Assert.That(apiDebts, Is.Not.Null);
        Assert.That(apiDebts.Count, Is.GreaterThanOrEqualTo(3));

        var testCategories = debts.ExceptBy(apiDebts.Select(x => x.Id), category => category.Id).ToArray();
        Assert.That(testCategories, Is.Not.Null);
        Assert.That(testCategories, Is.Empty);
    }

    [Test]
    public async Task GetByIdTest()
    {
        var debt = _user.WithDebt();
        _dbClient.Save();

        var apiDebt = await _apiClient.Debt.GetById(debt.Id).IsSuccessWithContent();

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

        var debt = _user.WithDebt();

        var request = new DebtClient.SaveRequest
        {
            Comment = debt.Comment,
            DebtUserName = debt.DebtUserName,
            Sum = debt.Sum,
            Date = debt.Date,
            TypeId = (int)debt.Type,
        };

        var createdCategoryId = await _apiClient.Debt.Create(request).IsSuccessWithContent();
        var dbDebt = _dbClient.CreateApplicationDbContext().Debts.SingleOrDefault(_user.Id, createdCategoryId);

        Assert.That(dbDebt, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(dbDebt.Comment, Is.EqualTo(debt.Comment));
            Assert.That(dbDebt.Date, Is.EqualTo(debt.Date));
            Assert.That(dbDebt.Sum, Is.EqualTo(debt.Sum));
            Assert.That(dbDebt.TypeId, Is.EqualTo((int)debt.Type));
        });
    }

    [Test]
    public async Task UpdateTest()
    {
        var debt = _user.WithDebt();
        _dbClient.Save();

        var updateDebt = _user.WithDebt();

        var request = new DebtClient.SaveRequest
        {
            Comment = updateDebt.Comment,
            Sum = updateDebt.Sum,
            DebtUserName = updateDebt.DebtUserName,
            Date = updateDebt.Date,
            TypeId = (int)updateDebt.Type,
        };

        await _apiClient.Debt.Update(debt.Id, request).IsSuccess();
        var dbDebt = _dbClient.CreateApplicationDbContext().Debts.SingleOrDefault(_user.Id, debt.Id);

        Assert.Multiple(() =>
        {
            Assert.That(dbDebt, Is.Not.Null);
        });

        Assert.Multiple(() =>
        {
            Assert.That(dbDebt.Comment, Is.EqualTo(updateDebt.Comment));
            Assert.That(dbDebt.Date, Is.EqualTo(updateDebt.Date));
            Assert.That(dbDebt.Sum, Is.EqualTo(updateDebt.Sum));
            Assert.That(dbDebt.TypeId, Is.EqualTo((int)updateDebt.Type));
        });
    }

    [Test]
    public async Task DeleteTest()
    {
        var debt = _user.WithDebt();
        _dbClient.Save();

        await _apiClient.Debt.Delete(debt.Id).IsSuccess();

        await using var context = _dbClient.CreateApplicationDbContext();

        var dbDebt = context.Debts.SingleOrDefault(_user.Id, debt.Id);

        Assert.That(dbDebt, Is.Null);

        dbDebt = context.Debts
            .IgnoreQueryFilters()
            .SingleOrDefault(_user.Id, debt.Id);

        Assert.That(dbDebt, Is.Not.Null);
        Assert.That(dbDebt.IsDeleted, Is.EqualTo(true));
    }

    [Test]
    public async Task RestoreTest()
    {
        var debt = _user.WithDebt().SetIsDeleted();
        _dbClient.Save();

        await _apiClient.Debt.Restore(debt.Id).IsSuccess();

        await using var context = _dbClient.CreateApplicationDbContext();

        var dbDebt = context.Debts.SingleOrDefault(_user.Id, debt.Id);
        Assert.That(dbDebt, Is.Not.Null);
        Assert.That(dbDebt.IsDeleted, Is.EqualTo(false));
    }
}
