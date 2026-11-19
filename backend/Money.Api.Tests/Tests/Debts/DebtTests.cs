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

        var apiDebts = await _apiClient.Debts.Get().IsSuccessWithContent();
        Assert.That(apiDebts, Is.Not.Null);
        Assert.That(apiDebts.Count, Is.GreaterThanOrEqualTo(3));

        var testCategories = debts.ExceptBy(apiDebts.Select(x => x.Id), category => category.Id).ToArray();
        Assert.That(testCategories, Is.Not.Null);
        Assert.That(testCategories, Is.Empty);
    }

    [Test]
    public async Task GetByIdTest()
    {
        var debtMovement = _user.WithDebt();
        _dbClient.Save();

        var apiDebt = await _apiClient.Debts.GetById(debtMovement.Id).IsSuccessWithContent();

        Assert.That(apiDebt, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(apiDebt.Id, Is.EqualTo(debtMovement.Id));
            Assert.That(apiDebt.Comment, Is.EqualTo(debtMovement.Comment));
            Assert.That(apiDebt.OwnerName, Is.EqualTo(debtMovement.OwnerName));
            Assert.That(apiDebt.Date, Is.EqualTo(debtMovement.Date));
            Assert.That(apiDebt.Sum, Is.EqualTo(debtMovement.Sum));
        });
    }

    [Test]
    public async Task CreateTest()
    {
        _dbClient.Save();

        var debtMovement = _user.WithDebt();

        var request = new DebtClient.SaveRequest
        {
            Comment = debtMovement.Comment,
            OwnerName = debtMovement.OwnerName,
            Sum = debtMovement.Sum,
            Date = debtMovement.Date,
            TypeId = (int)debtMovement.Type,
        };

        var createdCategoryId = await _apiClient.Debts.Create(request).IsSuccessWithContent();
        var dbDebt = await _dbClient.CreateApplicationDbContext().DebtMovements.FirstOrDefaultAsync(_user.Id, createdCategoryId);

        Assert.That(dbDebt, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(dbDebt.Comment, Is.EqualTo(debtMovement.Comment));
            Assert.That(dbDebt.Date, Is.EqualTo(debtMovement.Date));
            Assert.That(dbDebt.Sum, Is.EqualTo(debtMovement.Sum));
            Assert.That(dbDebt.TypeId, Is.EqualTo((int)debtMovement.Type));
        });
    }

    [Test]
    public async Task UpdateTest()
    {
        var debtMovement = _user.WithDebt();
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

        await _apiClient.Debts.Update(debtMovement.Id, request).IsSuccess();
        var dbDebt = await _dbClient.CreateApplicationDbContext().DebtMovements.FirstOrDefaultAsync(_user.Id, debtMovement.Id);

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
        var debtMovement = _user.WithDebt();
        _dbClient.Save();

        await _apiClient.Debts.Delete(debtMovement.Id).IsSuccess();

        await using var context = _dbClient.CreateApplicationDbContext();

        var dbDebt = await context.DebtMovements.FirstOrDefaultAsync(_user.Id, debtMovement.Id);

        Assert.That(dbDebt, Is.Null);

        dbDebt = await context.DebtMovements
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(_user.Id, debtMovement.Id);

        Assert.That(dbDebt, Is.Not.Null);
        Assert.That(dbDebt.IsDeleted, Is.EqualTo(true));
    }

    [Test]
    public async Task RestoreTest()
    {
        var debtMovement = _user.WithDebt().SetIsDeleted();
        _dbClient.Save();

        await _apiClient.Debts.Restore(debtMovement.Id).IsSuccess();

        await using var context = _dbClient.CreateApplicationDbContext();

        var dbDebt = await context.DebtMovements.FirstOrDefaultAsync(_user.Id, debtMovement.Id);
        Assert.That(dbDebt, Is.Not.Null);
        Assert.That(dbDebt.IsDeleted, Is.EqualTo(false));
    }

    [Test]
    public async Task PayFullTest()
    {
        var debtMovement = _user.WithDebt();
        _dbClient.Save();

        var request = new DebtClient.PayRequest
        {
            Sum = debtMovement.Sum,
            Comment = "Всё вернул в срок, красава",
            Date = DateTime.Now.Date,
        };

        await _apiClient.Debts.Pay(debtMovement.Id, request).IsSuccess();

        await using var context = _dbClient.CreateApplicationDbContext();

        var dbDebts = await context.DebtMovements
            .Where(x => x.UserId == _user.Id && x.OwnerId == debtMovement.OwnerId)
            .ToListAsync();

        Assert.Multiple(() =>
        {
            Assert.That(dbDebts, Has.Count.EqualTo(2));
            Assert.That(dbDebts.Sum(x => x.TypeId == (int)DebtTypes.Plus ? x.Sum : -x.Sum), Is.Zero);
            Assert.That(dbDebts.Single(x => x.Id != debtMovement.Id).Comment, Is.EqualTo(request.Comment));
        });
    }

    [Test]
    public async Task PayPartTest()
    {
        var debtMovement = _user.WithDebt().SetSum(100);
        _dbClient.Save();

        var request = new DebtClient.PayRequest
        {
            Sum = 20,
            Comment = "Всё вернул в срок, красава",
            Date = DateTime.Now.Date,
        };

        await _apiClient.Debts.Pay(debtMovement.Id, request).IsSuccess();

        await using var context = _dbClient.CreateApplicationDbContext();

        var dbDebts = await context.DebtMovements
            .Where(x => x.UserId == _user.Id && x.OwnerId == debtMovement.OwnerId)
            .ToListAsync();

        Assert.Multiple(() =>
        {
            Assert.That(dbDebts, Has.Count.EqualTo(2));
            Assert.That(dbDebts.Sum(x => x.TypeId == (int)DebtTypes.Plus ? x.Sum : -x.Sum), Is.EqualTo(80));
        });
    }

    [Test]
    public async Task OnePaymentClosesSeveralOwnerDebtsTest()
    {
        var first = _user.WithDebt().SetOwnerName("Общий держатель").SetType(DebtTypes.Plus).SetSum(1000);
        _user.WithDebt().SetOwnerName("Общий держатель").SetType(DebtTypes.Plus).SetSum(1000);
        _dbClient.Save();

        var request = new DebtClient.PayRequest
        {
            Sum = 2000,
            Comment = "Вернул всё одним переводом",
            Date = DateTime.Now.Date,
        };

        await _apiClient.Debts.Pay(first.Id, request).IsSuccess();

        await using var context = _dbClient.CreateApplicationDbContext();
        var debts = await context.DebtMovements
            .Where(x => x.UserId == _user.Id && x.OwnerId == first.OwnerId)
            .ToListAsync();

        Assert.That(debts, Has.Count.EqualTo(3));
        Assert.That(debts.Sum(x => x.TypeId == (int)DebtTypes.Plus ? x.Sum : -x.Sum), Is.Zero);
    }

    [Test]
    public async Task OnePaymentClosesFiveOwnerDebtsTest()
    {
        var debts = Enumerable.Range(0, 5)
            .Select(_ => _user.WithDebt()
                .SetOwnerName("Пять выдач")
                .SetType(DebtTypes.Plus)
                .SetSum(1000))
            .ToArray();
        _dbClient.Save();

        var request = new DebtClient.PayRequest
        {
            Sum = 5000,
            Comment = "Вернули пять тысяч одним переводом",
            Date = DateTime.Now.Date,
        };

        await _apiClient.Debts.Pay(debts[0].Id, request).IsSuccess();

        await using var context = _dbClient.CreateApplicationDbContext();
        var movements = await context.DebtMovements
            .Where(x => x.UserId == _user.Id && x.OwnerId == debts[0].OwnerId)
            .OrderBy(x => x.Id)
            .ToListAsync();

        var payment = movements.Single(x => x.Id != debts[0].Id
                                            && debts.All(debtMovement => debtMovement.Id != x.Id));

        Assert.Multiple(() =>
        {
            Assert.That(movements, Has.Count.EqualTo(6));
            Assert.That(payment.TypeId, Is.EqualTo((int)DebtTypes.Minus));
            Assert.That(payment.Sum, Is.EqualTo(5000));
            Assert.That(payment.Comment, Is.EqualTo(request.Comment));
            Assert.That(movements.Sum(SignedSum), Is.Zero);
        });
    }

    [Test]
    public async Task PaymentForPayableCreatesPositiveMovementTest()
    {
        var debtMovement = _user.WithDebt()
            .SetOwnerName("Я должен")
            .SetType(DebtTypes.Minus)
            .SetSum(1000);
        _dbClient.Save();

        var request = new DebtClient.PayRequest
        {
            Sum = 400,
            Comment = "Вернул часть",
            Date = DateTime.Now.Date,
        };

        await _apiClient.Debts.Pay(debtMovement.Id, request).IsSuccess();

        await using var context = _dbClient.CreateApplicationDbContext();
        var movements = await context.DebtMovements
            .Where(x => x.UserId == _user.Id && x.OwnerId == debtMovement.OwnerId)
            .ToListAsync();
        var payment = movements.Single(x => x.Id != debtMovement.Id);

        Assert.Multiple(() =>
        {
            Assert.That(payment.TypeId, Is.EqualTo((int)DebtTypes.Plus));
            Assert.That(payment.Sum, Is.EqualTo(400));
            Assert.That(movements.Sum(SignedSum), Is.EqualTo(-600));
        });
    }

    [Test]
    public async Task PaymentCannotExceedOwnerBalanceTest()
    {
        var debtMovement = _user.WithDebt()
            .SetOwnerName("Ограниченный остаток")
            .SetType(DebtTypes.Plus)
            .SetSum(1000);
        _user.WithDebt()
            .SetOwnerName("Ограниченный остаток")
            .SetType(DebtTypes.Minus)
            .SetSum(400);
        _dbClient.Save();

        var request = new DebtClient.PayRequest
        {
            Sum = 601,
            Comment = "Лишний рубль",
            Date = DateTime.Now.Date,
        };

        await _apiClient.Debts.Pay(debtMovement.Id, request).IsBadRequest();

        await using var context = _dbClient.CreateApplicationDbContext();
        var movements = await context.DebtMovements
            .Where(x => x.UserId == _user.Id && x.OwnerId == debtMovement.OwnerId)
            .ToListAsync();

        Assert.That(movements, Has.Count.EqualTo(2));
        Assert.That(movements.Sum(SignedSum), Is.EqualTo(600));
    }

    [Test]
    public async Task PaymentCannotUseDirectionOppositeToCurrentBalanceTest()
    {
        var positiveMovement = _user.WithDebt()
            .SetOwnerName("Сменившийся баланс")
            .SetType(DebtTypes.Plus)
            .SetSum(1000);
        _user.WithDebt()
            .SetOwnerName("Сменившийся баланс")
            .SetType(DebtTypes.Minus)
            .SetSum(1500);
        _dbClient.Save();

        var request = new DebtClient.PayRequest
        {
            Sum = 100,
            Comment = "Неверное направление",
            Date = DateTime.Now.Date,
        };

        await _apiClient.Debts.Pay(positiveMovement.Id, request).IsBadRequest();
    }

    [Test]
    public async Task OppositeMovementOffsetsOwnerBalanceTest()
    {
        var receivable = _user.WithDebt()
            .SetOwnerName("Встречный баланс")
            .SetType(DebtTypes.Plus)
            .SetSum(1000);
        _dbClient.Save();

        var request = new DebtClient.SaveRequest
        {
            Comment = "Теперь должен я",
            OwnerName = receivable.OwnerName,
            Sum = 1500,
            Date = DateTime.Now.Date,
            TypeId = (int)DebtTypes.Minus,
        };

        var payableId = await _apiClient.Debts.Create(request).IsSuccessWithContent();

        await using var context = _dbClient.CreateApplicationDbContext();
        var debts = await context.DebtMovements
            .Where(x => x.UserId == _user.Id && (x.Id == receivable.Id || x.Id == payableId))
            .ToDictionaryAsync(x => x.Id);

        Assert.Multiple(() =>
        {
            Assert.That(debts.Values.Sum(x => x.TypeId == (int)DebtTypes.Plus ? x.Sum : -x.Sum), Is.EqualTo(-500));
        });
    }

    [Test]
    public async Task EditMovementAfterCounterMovementTest()
    {
        var debtMovement = _user.WithDebt().SetSum(100);
        _dbClient.Save();

        var request = new DebtClient.PayRequest
        {
            Sum = 20,
            Comment = "Маловато вернул",
            Date = DateTime.Now.Date,
        };

        await _apiClient.Debts.Pay(debtMovement.Id, request).IsSuccess();

        var saveRequest = new DebtClient.SaveRequest
        {
            Comment = debtMovement.Comment,
            Sum = 10,
            OwnerName = debtMovement.OwnerName,
            Date = debtMovement.Date,
            TypeId = (int)debtMovement.Type,
        };

        await _apiClient.Debts.Update(debtMovement.Id, saveRequest).IsSuccess();

        await using var context = _dbClient.CreateApplicationDbContext();
        var dbDebt = await context.DebtMovements
            .IsUserEntity(_user.Id, debtMovement.Id)
            .FirstAsync();
        Assert.That(dbDebt.Sum, Is.EqualTo(10));
    }

    [Test]
    public async Task MergeOwnersTest()
    {
        var debt1 = _user.WithDebt().SetOwnerName("User1");
        var debt2 = _user.WithDebt().SetOwnerName("User2");
        _dbClient.Save();

        var fromUserId = debt1.OwnerId;
        var toUserId = debt2.OwnerId;

        await _apiClient.Debts.MergeOwners(fromUserId, toUserId).IsSuccess();

        await using var context = _dbClient.CreateApplicationDbContext();

        var dbDebts = await context.DebtMovements.Where(x => x.UserId == debt1.User.Id).ToListAsync();
        var dbDebtOwner = await context.DebtOwners.FirstOrDefaultAsync(x => x.UserId == debt1.User.Id && x.Id == fromUserId);

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
        var debtMovement = _user.WithDebt();
        _dbClient.Save();

        var owners = await _apiClient.Debts.GetOwners().IsSuccessWithContent();

        Assert.That(owners, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(owners, Has.Length.EqualTo(1));
            Assert.That(owners[0].Name, Is.EqualTo(debtMovement.OwnerName));
        });
    }

    [Test]
    public async Task ForgiveTest()
    {
        var debtMovement = _user.WithDebt().SetType(DebtTypes.Plus);
        var category = _user.WithCategory().SetOperationType(OperationTypes.Costs);
        _dbClient.Save();

        var comment = "Прощаю";
        await _apiClient.Debts.Forgive([debtMovement.Id], category.Id, comment).IsSuccess();

        await using var context = _dbClient.CreateApplicationDbContext();

        var dbOperations = await context.Operations
            .Where(x => x.UserId == debtMovement.User.Id)
            .ToListAsync();

        Assert.That(dbOperations, Is.Not.Null);
        Assert.That(dbOperations, Has.Count.EqualTo(1));

        Assert.Multiple(() =>
        {
            Assert.That(dbOperations[0].Date, Is.EqualTo(debtMovement.Date));
            Assert.That(dbOperations[0].Sum, Is.EqualTo(debtMovement.Sum));
            Assert.That(dbOperations[0].Comment?.Contains(comment, StringComparison.Ordinal), Is.True);
            Assert.That(dbOperations[0].CategoryId, Is.EqualTo(category.Id));
        });
    }

    [Test]
    public async Task ForgiveUsesNetOwnerBalanceAndCreatesOneClosingMovementTest()
    {
        var first = _user.WithDebt()
            .SetOwnerName("Чистый остаток")
            .SetType(DebtTypes.Plus)
            .SetSum(1000);
        var second = _user.WithDebt()
            .SetOwnerName("Чистый остаток")
            .SetType(DebtTypes.Plus)
            .SetSum(500);
        _user.WithDebt()
            .SetOwnerName("Чистый остаток")
            .SetType(DebtTypes.Minus)
            .SetSum(400);
        var category = _user.WithCategory().SetOperationType(OperationTypes.Costs);
        _dbClient.Save();

        await _apiClient.Debts
            .Forgive([first.Id, second.Id], category.Id, "Прощаю чистый остаток")
            .IsSuccess();

        await using var context = _dbClient.CreateApplicationDbContext();
        var operations = await context.Operations
            .Where(x => x.UserId == _user.Id)
            .ToListAsync();
        var movements = await context.DebtMovements
            .Where(x => x.UserId == _user.Id && x.OwnerId == first.OwnerId)
            .ToListAsync();

        Assert.Multiple(() =>
        {
            Assert.That(operations, Has.Count.EqualTo(1));
            Assert.That(operations[0].Sum, Is.EqualTo(1100));
            Assert.That(movements, Has.Count.EqualTo(4));
            Assert.That(movements.Count(x => x.TypeId == (int)DebtTypes.Minus && x.Sum == 1100), Is.EqualTo(1));
            Assert.That(movements.Sum(SignedSum), Is.Zero);
        });
    }

    private static decimal SignedSum(Money.Data.Entities.DebtMovement movement)
    {
        return movement.TypeId == (int)DebtTypes.Plus ? movement.Sum : -movement.Sum;
    }
}
