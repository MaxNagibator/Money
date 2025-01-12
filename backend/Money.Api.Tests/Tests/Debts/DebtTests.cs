using Microsoft.EntityFrameworkCore;
using Money.ApiClient;
using Money.Business.Enums;
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

        var apiDebts = await _apiClient.Debt.Get().IsSuccessWithContent();
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
            Assert.That(apiDebt.OwnerName, Is.EqualTo(debt.OwnerName));
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
            OwnerName = debt.OwnerName,
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
            OwnerName = updateDebt.OwnerName,
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

    [Test]
    public async Task PayFullTest()
    {
        var debt = _user.WithDebt();
        _dbClient.Save();

        var request = new DebtClient.PayRequest
        {
            Sum = debt.Sum,
            Comment = "Всё вернул в срок, красава",
            Date = DateTime.Now.Date,
        };

        await _apiClient.Debt.Pay(debt.Id, request).IsSuccess();

        await using var context = _dbClient.CreateApplicationDbContext();

        var dbDebt = context.Debts.Single(_user.Id, debt.Id);

        Assert.Multiple(() =>
        {
            Assert.That(dbDebt.PaySum, Is.EqualTo(debt.Sum));
            Assert.That(dbDebt.StatusId, Is.EqualTo((int)DebtStatus.Paid));
        });
    }

    [Test]
    public async Task PayPartTest()
    {
        var debt = _user.WithDebt().SetSum(100);
        _dbClient.Save();

        var request = new DebtClient.PayRequest
        {
            Sum = 20,
            Comment = "Всё вернул в срок, красава",
            Date = DateTime.Now.Date,
        };

        await _apiClient.Debt.Pay(debt.Id, request).IsSuccess();

        await using var context = _dbClient.CreateApplicationDbContext();

        var dbDebt = context.Debts.Single(_user.Id, debt.Id);

        Assert.Multiple(() =>
        {
            Assert.That(dbDebt.PaySum, Is.EqualTo(request.Sum));
            Assert.That(dbDebt.StatusId, Is.EqualTo((int)DebtStatus.Actual));
        });
    }

    [Test]
    public async Task EditPayedDebtWithOverflowPaySumTest()
    {
        var debt = _user.WithDebt().SetSum(100);
        _dbClient.Save();

        var request = new DebtClient.PayRequest
        {
            Sum = 20,
            Comment = "Маловато вернул",
            Date = DateTime.Now.Date,
        };

        await _apiClient.Debt.Pay(debt.Id, request).IsSuccess();

        var saveRequest = new DebtClient.SaveRequest
        {
            Comment = debt.Comment,
            Sum = 10,
            OwnerName = debt.OwnerName,
            Date = debt.Date,
            TypeId = (int)debt.Type,
        };

        await _apiClient.Debt.Update(debt.Id, saveRequest).IsBadRequest();
    }

    [Test]
    public async Task MergeOwnersTest()
    {
        var debt1 = _user.WithDebt().SetOwnerName("User1");
        var debt2 = _user.WithDebt().SetOwnerName("User2");
        _dbClient.Save();

        var fromUserId = debt1.OwnerId;
        var toUserId = debt2.OwnerId;

        await _apiClient.Debt.MergeOwners(fromUserId, toUserId).IsSuccess();

        await using var context = _dbClient.CreateApplicationDbContext();

        var dbDebts = context.Debts.Where(x => x.UserId == debt1.User.Id).ToList();
        var dbDebtOwner = context.DebtOwners.FirstOrDefault(x => x.UserId == debt1.User.Id && x.Id == fromUserId);

        Assert.Multiple(() =>
        {
            Assert.That(dbDebtOwner, Is.Null);
            Assert.That(dbDebts, Has.Count.EqualTo(2));
        });

        Assert.Multiple(() =>
        {
            Assert.That(dbDebts[0].OwnerId, Is.EqualTo(toUserId));
            Assert.That(dbDebts[1].OwnerId, Is.EqualTo(toUserId));
        });
    }

    [Test]
    public async Task GetOwnersTest()
    {
        var debt = _user.WithDebt();
        _dbClient.Save();

        var owners = await _apiClient.Debt.GetOwners().IsSuccessWithContent();

        Assert.That(owners, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(owners, Has.Length.EqualTo(1));
            Assert.That(owners[0].Name, Is.EqualTo(debt.OwnerName));
        });
    }

    [Test]
    public async Task ForgiveTest()
    {
        var debt = _user.WithDebt().SetType(DebtTypes.Plus);
        var category = _user.WithCategory().SetOperationType(OperationTypes.Costs);
        _dbClient.Save();

        var comment = "Прощаю";
        await _apiClient.Debt.Forgive([debt.Id], category.Id, comment).IsSuccess();

        await using var context = _dbClient.CreateApplicationDbContext();

        var dbOperations = context.Operations
            .Where(x => x.UserId == debt.User.Id)
            .ToList();

        Assert.That(dbOperations, Is.Not.Null);
        Assert.That(dbOperations, Has.Count.EqualTo(1));

        Assert.Multiple(() =>
        {
            Assert.That(dbOperations[0].Date, Is.EqualTo(debt.Date));
            Assert.That(dbOperations[0].Sum, Is.EqualTo(debt.Sum));
            Assert.That(dbOperations[0].Comment?.Contains(comment), Is.True);
            Assert.That(dbOperations[0].CategoryId, Is.EqualTo(category.Id));
        });
    }
}
