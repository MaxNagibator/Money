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
    public async Task OppositeMovementsKeepIndependentHistoryTest()
    {
        _dbClient.Save();
        var date = DateTime.Today;
        var ownerName = $"Расчёт {_user.Id}";
        var outgoingId = await _apiClient.Debts.Create(new()
        {
            TypeId = (int)DebtTypes.Plus,
            Sum = 1000,
            OwnerName = ownerName,
            Date = date,
            Comment = "Выдал",
        }).IsSuccessWithContent();
        var incomingId = await _apiClient.Debts.Create(new()
        {
            TypeId = (int)DebtTypes.Minus,
            Sum = 1200,
            OwnerName = ownerName,
            Date = date,
            Comment = "Возврат с переплатой",
        }).IsSuccessWithContent();

        await _apiClient.Debts.Update(incomingId, new()
        {
            TypeId = (int)DebtTypes.Minus,
            Sum = 900,
            OwnerName = ownerName,
            Date = date.AddDays(-1),
            Comment = "Исправленный возврат",
        }).IsSuccess();

        await using var context = _dbClient.CreateApplicationDbContext();
        var entries = await context.Debts.IsUserEntity(_user.Id).OrderBy(x => x.Id).ToListAsync();
        var outgoing = entries.Single(x => x.Id == outgoingId);
        var incoming = entries.Single(x => x.Id == incomingId);

        Assert.Multiple(() =>
        {
            Assert.That(entries, Has.Count.EqualTo(2));
            Assert.That(outgoing.OwnerId, Is.EqualTo(incoming.OwnerId));
            Assert.That(outgoing.Sum, Is.EqualTo(1000));
            Assert.That(outgoing.PaySum, Is.Zero);
            Assert.That(outgoing.Comment, Is.EqualTo("Выдал"));
            Assert.That(incoming.Sum, Is.EqualTo(900));
            Assert.That(incoming.PaySum, Is.Zero);
            Assert.That(incoming.Date, Is.EqualTo(date.AddDays(-1)));
            Assert.That(incoming.Comment, Is.EqualTo("Исправленный возврат"));
        });
    }

    [Test]
    public async Task GetByIdTest()
    {
        var debt = _user.WithDebt();
        _dbClient.Save();

        var apiDebt = await _apiClient.Debts.GetById(debt.Id).IsSuccessWithContent();

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
    public async Task ForgiveAccountDeletesMovementsAndExpensesOnlyNetBalanceTest()
    {
        var outgoing = _user.WithDebt().SetType(DebtTypes.Plus).SetSum(1000).SetOwnerName("Иван");
        _user.WithDebt().SetType(DebtTypes.Minus).SetSum(400).SetOwnerName("Иван");
        var paid = _user.WithDebt().SetType(DebtTypes.Plus).SetSum(300).SetOwnerName("Иван");
        var other = _user.WithDebt().SetType(DebtTypes.Plus).SetSum(200).SetOwnerName("Анна");
        var category = _user.WithCategory().SetOperationType(OperationTypes.Costs);
        _dbClient.Save();

        await _apiClient.Debts.Pay(paid.Id, new() { Sum = 300, Date = DateTime.Today }).IsSuccess();

        await _apiClient.Debts.ForgiveAccount(outgoing.Id, category.Id, new()
        {
            Sum = 600,
            Date = DateTime.Today,
            Comment = "Списать остаток",
        }).IsSuccess();

        await using var context = _dbClient.CreateApplicationDbContext();
        var entries = await context.Debts.IgnoreQueryFilters().IsUserEntity(_user.Id).ToListAsync();
        var expenses = await context.Operations.IsUserEntity(_user.Id).ToListAsync();
        var apiEntries = await _apiClient.Debts.Get(withPaid: true).IsSuccessWithContent();
        Assert.That(apiEntries, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(entries.Select(x => x.Id), Is.EqualTo(new[] { other.Id }));
            Assert.That(apiEntries.Select(x => x.Id), Is.EqualTo(new[] { other.Id }));
            Assert.That(expenses, Has.Count.EqualTo(1));
            Assert.That(expenses[0].Sum, Is.EqualTo(600));
            Assert.That(expenses[0].Date, Is.EqualTo(DateTime.Today));
        });

        var repeated = await _apiClient.Debts.ForgiveAccount(outgoing.Id, category.Id, new()
        {
            Sum = 600,
            Date = DateTime.Today,
        });
        Assert.That(repeated.Code, Is.EqualTo(System.Net.HttpStatusCode.NotFound));
        Assert.That(await context.Operations.IsUserEntity(_user.Id).CountAsync(), Is.EqualTo(1));
    }

    [TestCase(599, false)]
    [TestCase(600, true)]
    public async Task InvalidAccountWriteOffDoesNotLeaveExpenseTest(decimal expectedBalance, bool incomeCategory)
    {
        var debt = _user.WithDebt().SetType(DebtTypes.Plus).SetSum(600);
        var category = _user.WithCategory().SetOperationType(incomeCategory ? OperationTypes.Income : OperationTypes.Costs);
        _dbClient.Save();

        await _apiClient.Debts.ForgiveAccount(debt.Id, category.Id, new()
        {
            Sum = expectedBalance,
            Date = DateTime.Today,
        }).IsBadRequest();

        await using var context = _dbClient.CreateApplicationDbContext();
        Assert.Multiple(() =>
        {
            Assert.That(context.Debts.IsUserEntity(_user.Id).Count(), Is.EqualTo(1));
            Assert.That(context.Operations.IsUserEntity(_user.Id).Count(), Is.Zero);
        });
    }

    [Test]
    public async Task ConcurrentAccountWriteOffCreatesOneExpenseTest()
    {
        var debt = _user.WithDebt().SetType(DebtTypes.Plus).SetSum(600);
        var category = _user.WithCategory().SetOperationType(OperationTypes.Costs);
        _dbClient.Save();
        var request = new DebtClient.PayRequest { Sum = 600, Date = DateTime.Today };
        var responses = await Task.WhenAll(
            _apiClient.Debts.ForgiveAccount(debt.Id, category.Id, request),
            _apiClient.Debts.ForgiveAccount(debt.Id, category.Id, request));

        await using var context = _dbClient.CreateApplicationDbContext();
        Assert.Multiple(() =>
        {
            Assert.That(responses.Count(response => response.IsSuccessStatusCode), Is.EqualTo(1));
            Assert.That(responses.Count(response => response.Code is System.Net.HttpStatusCode.BadRequest or System.Net.HttpStatusCode.NotFound), Is.EqualTo(1));
            Assert.That(context.Operations.IsUserEntity(_user.Id).Count(), Is.EqualTo(1));
            Assert.That(context.Debts.IgnoreQueryFilters().IsUserEntity(_user.Id).Count(), Is.Zero);
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

        var createdCategoryId = await _apiClient.Debts.Create(request).IsSuccessWithContent();
        var dbDebt = await _dbClient.CreateApplicationDbContext().Debts.FirstOrDefaultAsync(_user.Id, createdCategoryId);

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

        await _apiClient.Debts.Update(debt.Id, request).IsSuccess();
        var dbDebt = await _dbClient.CreateApplicationDbContext().Debts.FirstOrDefaultAsync(_user.Id, debt.Id);

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

        await _apiClient.Debts.Delete(debt.Id).IsSuccess();

        await using var context = _dbClient.CreateApplicationDbContext();

        var dbDebt = await context.Debts.FirstOrDefaultAsync(_user.Id, debt.Id);

        Assert.That(dbDebt, Is.Null);

        dbDebt = await context.Debts
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(_user.Id, debt.Id);

        Assert.That(dbDebt, Is.Not.Null);
        Assert.That(dbDebt.IsDeleted, Is.EqualTo(true));
    }

    [Test]
    public async Task RestoreTest()
    {
        var debt = _user.WithDebt().SetIsDeleted();
        _dbClient.Save();

        await _apiClient.Debts.Restore(debt.Id).IsSuccess();

        await using var context = _dbClient.CreateApplicationDbContext();

        var dbDebt = await context.Debts.FirstOrDefaultAsync(_user.Id, debt.Id);
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

        await _apiClient.Debts.Pay(debt.Id, request).IsSuccess();

        await using var context = _dbClient.CreateApplicationDbContext();

        var dbDebt = context.Debts.First(_user.Id, debt.Id);

        Assert.Multiple(() =>
        {
            Assert.That(dbDebt.PaySum, Is.EqualTo(debt.Sum));
            Assert.That(dbDebt.StatusId, Is.EqualTo((int)DebtStatus.Paid));
        });
    }

    [Test]
    public async Task PayOwnerDistributesOldestFirstWithinOneDirectionTest()
    {
        const string ownerName = "Общий возврат";
        var newest = _user.WithDebt().SetOwnerName(ownerName).SetType(DebtTypes.Plus).SetSum(100);
        var oldest = _user.WithDebt().SetOwnerName(ownerName).SetType(DebtTypes.Plus).SetSum(500);
        var sameDate = _user.WithDebt().SetOwnerName(ownerName).SetType(DebtTypes.Plus).SetSum(300);
        var opposite = _user.WithDebt().SetOwnerName(ownerName).SetType(DebtTypes.Minus).SetSum(1000);
        var deleted = _user.WithDebt().SetOwnerName(ownerName).SetType(DebtTypes.Plus).SetSum(900).SetIsDeleted();
        var foreign = _dbClient.WithUser().WithDebt().SetOwnerName(ownerName).SetType(DebtTypes.Plus).SetSum(700);
        _dbClient.Save();

        await using (var context = _dbClient.CreateApplicationDbContext())
        {
            var entries = await context.Debts.IsUserEntity(_user.Id).ToListAsync();
            entries.Single(x => x.Id == oldest.Id).Date = DateTime.Today.AddDays(-2);
            entries.Single(x => x.Id == sameDate.Id).Date = DateTime.Today.AddDays(-2);
            await context.SaveChangesAsync();
        }

        await _apiClient.Debts.Pay(oldest.Id, new() { Sum = 100, Date = DateTime.Today, Comment = "Первый возврат" }).IsSuccess();
        await _apiClient.Debts.PayOwner(newest.Id, new() { Sum = 550, Date = DateTime.Today, Comment = "Общий возврат", OwnerName = ownerName, TypeId = (int)DebtTypes.Plus }).IsSuccess();

        await using var result = _dbClient.CreateApplicationDbContext();
        var debts = await result.Debts.IgnoreQueryFilters().IsUserEntity(_user.Id).ToListAsync();
        var paid = debts.Single(x => x.Id == oldest.Id);
        var partial = debts.Single(x => x.Id == sameDate.Id);
        var foreignDebt = await result.Debts.IsUserEntity(foreign.User.Id, foreign.Id).SingleAsync();
        Assert.Multiple(() =>
        {
            Assert.That(debts, Has.Count.EqualTo(5));
            Assert.That(paid.PaySum, Is.EqualTo(500));
            Assert.That(paid.StatusId, Is.EqualTo((int)DebtStatus.Paid));
            Assert.That(paid.PayComment, Does.Contain("Первый возврат").And.Contain("Общий возврат"));
            Assert.That(partial.PaySum, Is.EqualTo(150));
            Assert.That(partial.StatusId, Is.EqualTo((int)DebtStatus.Actual));
            Assert.That(partial.PayComment, Does.Contain("Общий возврат"));
            Assert.That(debts.Single(x => x.Id == newest.Id).PaySum, Is.Zero);
            Assert.That(debts.Single(x => x.Id == opposite.Id).PaySum, Is.Zero);
            Assert.That(debts.Single(x => x.Id == deleted.Id).PaySum, Is.Zero);
            Assert.That(foreignDebt.PaySum, Is.Zero);
        });
    }

    [Test]
    public async Task PayOwnerRejectsOverpaymentWithoutChangingDebtsTest()
    {
        var debt = _user.WithDebt().SetOwnerName("Без взаимозачёта").SetType(DebtTypes.Plus).SetSum(100);
        _user.WithDebt().SetOwnerName(debt.OwnerName).SetType(DebtTypes.Minus).SetSum(1000);
        _dbClient.Save();

        var response = await _apiClient.Debts.PayOwner(debt.Id, new() { Sum = 101, Date = DateTime.Today, OwnerName = debt.OwnerName, TypeId = (int)debt.Type });

        await using var context = _dbClient.CreateApplicationDbContext();
        var entries = await context.Debts.IsUserEntity(_user.Id).ToListAsync();
        Assert.Multiple(() =>
        {
            Assert.That(response.Code, Is.EqualTo(System.Net.HttpStatusCode.BadRequest));
            Assert.That(entries, Has.Count.EqualTo(2));
            Assert.That(entries.Sum(x => x.PaySum), Is.Zero);
        });
    }

    [TestCase(0)]
    [TestCase(-1)]
    public async Task PayOwnerRejectsNonpositiveAmountTest(decimal amount)
    {
        var debt = _user.WithDebt().SetSum(100);
        _dbClient.Save();

        var response = await _apiClient.Debts.PayOwner(debt.Id, new() { Sum = amount, Date = DateTime.Today, OwnerName = debt.OwnerName, TypeId = (int)debt.Type });

        Assert.That(response.Code, Is.EqualTo(System.Net.HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task PayOwnerRejectsMissingDateTest()
    {
        var debt = _user.WithDebt().SetSum(100);
        _dbClient.Save();

        var response = await _apiClient.Debts.PayOwner(debt.Id, new() { Sum = 10, OwnerName = debt.OwnerName, TypeId = (int)debt.Type });

        Assert.That(response.Code, Is.EqualTo(System.Net.HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task PayOwnerCannotUseAnotherUsersDebtTest()
    {
        var foreign = _dbClient.WithUser().WithDebt().SetSum(100);
        _dbClient.Save();

        var response = await _apiClient.Debts.PayOwner(foreign.Id, new() { Sum = 10, Date = DateTime.Today, OwnerName = foreign.OwnerName, TypeId = (int)foreign.Type });

        Assert.That(response.Code, Is.EqualTo(System.Net.HttpStatusCode.NotFound));
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task ConcurrentPayOwnerAndPaymentDoNotLosePaymentsTest(bool singlePayment)
    {
        var debt = _user.WithDebt().SetSum(600);
        _dbClient.Save();
        var request = new DebtClient.PayOwnerRequest { Sum = 400, Date = DateTime.Today, OwnerName = debt.OwnerName, TypeId = (int)debt.Type };

        var responses = await Task.WhenAll(
            _apiClient.Debts.PayOwner(debt.Id, request),
            singlePayment ? _apiClient.Debts.Pay(debt.Id, request) : _apiClient.Debts.PayOwner(debt.Id, request));

        await using var context = _dbClient.CreateApplicationDbContext();
        var updated = await context.Debts.IsUserEntity(_user.Id, debt.Id).SingleAsync();
        Assert.Multiple(() =>
        {
            Assert.That(responses.Count(x => x.Code == System.Net.HttpStatusCode.NoContent), Is.EqualTo(1));
            Assert.That(responses.Count(x => x.Code == System.Net.HttpStatusCode.BadRequest), Is.EqualTo(1));
            Assert.That(updated.PaySum, Is.EqualTo(400));
        });
    }

    [Test]
    public async Task PayOwnerRollsBackWhenPaymentHistoryIsFullTest()
    {
        var first = _user.WithDebt().SetOwnerName("Атомарный возврат").SetSum(100).SetType(DebtTypes.Plus);
        var second = _user.WithDebt().SetOwnerName(first.OwnerName).SetSum(100).SetType(DebtTypes.Plus);
        _dbClient.Save();
        await using (var context = _dbClient.CreateApplicationDbContext())
        {
            var entry = await context.Debts.IsUserEntity(_user.Id, second.Id).SingleAsync();
            entry.PayComment = new string('я', 4000);
            await context.SaveChangesAsync();
        }

        var response = await _apiClient.Debts.PayOwner(first.Id, new() { Sum = 150, Date = DateTime.Today, OwnerName = first.OwnerName, TypeId = (int)first.Type });

        await using var result = _dbClient.CreateApplicationDbContext();
        var entries = await result.Debts.IsUserEntity(_user.Id).ToListAsync();
        Assert.Multiple(() =>
        {
            Assert.That(response.Code, Is.EqualTo(System.Net.HttpStatusCode.BadRequest));
            Assert.That(entries.Sum(x => x.PaySum), Is.Zero);
            Assert.That(entries.All(x => x.StatusId == (int)DebtStatus.Actual), Is.True);
        });
    }

    [Test]
    public async Task PayOwnerClosesDebtsAcrossDuplicateOwnerRecordsWithSameNameTest()
    {
        const string ownerName = "Дублированный";
        var first = _user.WithDebt().SetOwnerName(ownerName).SetType(DebtTypes.Plus).SetSum(600);
        var foreign = _dbClient.WithUser().WithDebt().SetOwnerName(ownerName).SetType(DebtTypes.Plus).SetSum(500);
        _dbClient.Save();

        int secondId;
        int oppositeId;
        await using (var context = _dbClient.CreateApplicationDbContext())
        {
            var domainUser = await context.DomainUsers.SingleAsync(x => x.Id == _user.Id);
            var duplicateOwnerId = ++domainUser.NextDebtOwnerId;
            context.DebtOwners.Add(new Money.Data.Entities.DebtOwner
            {
                Id = duplicateOwnerId,
                UserId = _user.Id,
                Name = ownerName,
            });
            secondId = ++domainUser.NextDebtId;
            context.Debts.Add(new Money.Data.Entities.Debt
            {
                Id = secondId,
                UserId = _user.Id,
                OwnerId = duplicateOwnerId,
                TypeId = (int)DebtTypes.Plus,
                Sum = 600,
                Date = DateTime.Today.AddDays(-1),
                PaySum = 0,
                StatusId = (int)DebtStatus.Actual,
            });
            oppositeId = ++domainUser.NextDebtId;
            context.Debts.Add(new Money.Data.Entities.Debt
            {
                Id = oppositeId,
                UserId = _user.Id,
                OwnerId = duplicateOwnerId,
                TypeId = (int)DebtTypes.Minus,
                Sum = 400,
                Date = DateTime.Today.AddDays(-2),
                PaySum = 0,
                StatusId = (int)DebtStatus.Actual,
            });
            await context.SaveChangesAsync();
        }

        await _apiClient.Debts.PayOwner(first.Id, new()
        {
            Sum = 1200,
            Date = DateTime.Today,
            Comment = "Оплата дублей",
            OwnerName = ownerName,
            TypeId = (int)DebtTypes.Plus,
        }).IsSuccess();

        await using var result = _dbClient.CreateApplicationDbContext();
        var entries = await result.Debts.IsUserEntity(_user.Id).ToListAsync();
        var foreignDebt = await result.Debts.IsUserEntity(foreign.User.Id, foreign.Id).SingleAsync();
        Assert.Multiple(() =>
        {
            Assert.That(entries.Single(x => x.Id == first.Id).StatusId, Is.EqualTo((int)DebtStatus.Paid));
            Assert.That(entries.Single(x => x.Id == secondId).StatusId, Is.EqualTo((int)DebtStatus.Paid));
            Assert.That(entries.Single(x => x.Id == oppositeId).PaySum, Is.Zero);
            Assert.That(foreignDebt.PaySum, Is.Zero);
        });
    }

    [Test]
    public async Task PayOwnerRejectsStaleRequestWhenDebtOwnerChangedTest()
    {
        var debt = _user.WithDebt().SetOwnerName("Иван").SetType(DebtTypes.Plus).SetSum(500);
        _user.WithDebt().SetOwnerName("Пётр").SetType(DebtTypes.Plus).SetSum(500);
        _dbClient.Save();

        await _apiClient.Debts.Update(debt.Id, new()
        {
            TypeId = (int)DebtTypes.Plus,
            Sum = debt.Sum,
            OwnerName = "Пётр",
            Date = debt.Date,
            Comment = debt.Comment,
        }).IsSuccess();

        var response = await _apiClient.Debts.PayOwner(debt.Id, new()
        {
            Sum = 200,
            Date = DateTime.Today,
            OwnerName = "Иван",
            TypeId = (int)DebtTypes.Plus,
        });

        await using var context = _dbClient.CreateApplicationDbContext();
        var entries = await context.Debts.IsUserEntity(_user.Id).ToListAsync();
        Assert.Multiple(() =>
        {
            Assert.That(response.Code, Is.EqualTo(System.Net.HttpStatusCode.BadRequest));
            Assert.That(response.StringContent, Does.Contain("обновите страницу"));
            Assert.That(entries.Sum(x => x.PaySum), Is.Zero);
        });
    }

    [Test]
    public async Task PayOwnerRejectsStaleRequestWhenDebtDirectionChangedTest()
    {
        var debt = _user.WithDebt().SetOwnerName("Иван").SetType(DebtTypes.Plus).SetSum(500);
        _user.WithDebt().SetOwnerName("Иван").SetType(DebtTypes.Minus).SetSum(500);
        _dbClient.Save();

        await _apiClient.Debts.Update(debt.Id, new()
        {
            TypeId = (int)DebtTypes.Minus,
            Sum = debt.Sum,
            OwnerName = "Иван",
            Date = debt.Date,
            Comment = debt.Comment,
        }).IsSuccess();

        var response = await _apiClient.Debts.PayOwner(debt.Id, new()
        {
            Sum = 200,
            Date = DateTime.Today,
            OwnerName = "Иван",
            TypeId = (int)DebtTypes.Plus,
        });

        await using var context = _dbClient.CreateApplicationDbContext();
        var entries = await context.Debts.IsUserEntity(_user.Id).ToListAsync();
        Assert.Multiple(() =>
        {
            Assert.That(response.Code, Is.EqualTo(System.Net.HttpStatusCode.BadRequest));
            Assert.That(response.StringContent, Does.Contain("обновите страницу"));
            Assert.That(entries.Sum(x => x.PaySum), Is.Zero);
        });
    }

    [Test]
    public async Task PayOwnerRejectsStaleRequestWhenSourceDebtAlreadyClosedTest()
    {
        var oldest = _user.WithDebt().SetOwnerName("Иван").SetType(DebtTypes.Plus).SetSum(500);
        var newer = _user.WithDebt().SetOwnerName("Иван").SetType(DebtTypes.Plus).SetSum(400);
        _dbClient.Save();

        await _apiClient.Debts.Pay(oldest.Id, new() { Sum = 500, Date = DateTime.Today }).IsSuccess();

        var response = await _apiClient.Debts.PayOwner(oldest.Id, new()
        {
            Sum = 200,
            Date = DateTime.Today,
            OwnerName = "Иван",
            TypeId = (int)DebtTypes.Plus,
        });

        await using var context = _dbClient.CreateApplicationDbContext();
        var newerDb = await context.Debts.IsUserEntity(_user.Id, newer.Id).SingleAsync();
        Assert.Multiple(() =>
        {
            Assert.That(response.Code, Is.EqualTo(System.Net.HttpStatusCode.BadRequest));
            Assert.That(response.StringContent, Does.Contain("обновите страницу"));
            Assert.That(newerDb.PaySum, Is.Zero);
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

        await _apiClient.Debts.Pay(debt.Id, request).IsSuccess();

        await using var context = _dbClient.CreateApplicationDbContext();

        var dbDebt = context.Debts.First(_user.Id, debt.Id);

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

        await _apiClient.Debts.Pay(debt.Id, request).IsSuccess();

        var saveRequest = new DebtClient.SaveRequest
        {
            Comment = debt.Comment,
            Sum = 10,
            OwnerName = debt.OwnerName,
            Date = debt.Date,
            TypeId = (int)debt.Type,
        };

        await _apiClient.Debts.Update(debt.Id, saveRequest).IsBadRequest();
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

        var dbDebts = await context.Debts.Where(x => x.UserId == debt1.User.Id).ToListAsync();
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
        var debt = _user.WithDebt();
        _dbClient.Save();

        var owners = await _apiClient.Debts.GetOwners().IsSuccessWithContent();

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
        await _apiClient.Debts.Forgive([debt.Id], category.Id, comment).IsSuccess();

        await using var context = _dbClient.CreateApplicationDbContext();

        var dbOperations = await context.Operations
            .Where(x => x.UserId == debt.User.Id)
            .ToListAsync();

        Assert.That(dbOperations, Is.Not.Null);
        Assert.That(dbOperations, Has.Count.EqualTo(1));

        Assert.Multiple(() =>
        {
            Assert.That(dbOperations[0].Date, Is.EqualTo(debt.Date));
            Assert.That(dbOperations[0].Sum, Is.EqualTo(debt.Sum));
            Assert.That(dbOperations[0].Comment?.Contains(comment, StringComparison.Ordinal), Is.True);
            Assert.That(dbOperations[0].CategoryId, Is.EqualTo(category.Id));
            Assert.That(context.Debts.IgnoreQueryFilters().IsUserEntity(_user.Id).Count(), Is.Zero);
        });
    }
}
