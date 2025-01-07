using Money.Business.Enums;
using Money.Data.Extensions;
using Debt = Money.Data.Entities.Debt;

namespace Money.Api.Tests.TestTools.Entities;

/// <summary>
/// Долг.
/// </summary>
public class TestDebt : TestObject
{
    public TestDebt(TestUser user)
    {
        User = user;

        Type = TestRandom.GetEnum<DebtTypes>(); // todo сделать рандом для энумов
        Sum = TestRandom.GetInt();
        Comment = TestRandom.GetString("Comment");
        PayComment = TestRandom.GetString("PayComment");
        DebtUserName = TestRandom.GetString("User");

        Date = DateTime.Now.Date;
        Status = DebtStatus.Actual;
        PaySum = 0;
    }

    /// <summary>
    /// Пользователь.
    /// </summary>
    public TestUser User { get; }

    public int Id { get; private set; }

    public DebtTypes Type { get; }

    public decimal Sum { get; }

    public string? Comment { get; }

    public string DebtUserName { get; }

    public DateTime Date { get; }

    public decimal PaySum { get; }

    public string? PayComment { get; }

    public DebtStatus Status { get; }

    /// <summary>
    /// Удален.
    /// </summary>
    public bool IsDeleted { get; private set; }

    public TestDebt SetIsDeleted()
    {
        IsDeleted = true;
        return this;
    }

    private void FillDbProperties(Debt dbDebt, int debtUserId)
    {
        dbDebt.Sum = Sum;
        dbDebt.TypeId = (int)Type;
        dbDebt.DebtUserId = debtUserId;
        dbDebt.Comment = Comment;
        dbDebt.UserId = User.Id;
        dbDebt.Date = Date;
        dbDebt.PaySum = 0;
        dbDebt.StatusId = (int)Status;
        dbDebt.IsDeleted = IsDeleted;
    }

    public override void LocalSave()
    {
        var dbUser = Environment.Context.DomainUsers.Single(x => x.Id == User.Id);

        var dbDebtUser = Environment.Context.DebtUsers
            .SingleOrDefault(x => x.UserId == User.Id && x.Name == DebtUserName);

        if (dbDebtUser == null)
        {
            var debtUserId = dbUser.NextDebtUserId;
            dbUser.NextDebtUserId++;

            dbDebtUser = new()
            {
                Name = DebtUserName,
                UserId = User.Id,
                Id = debtUserId,
            };

            Environment.Context.DebtUsers.Add(dbDebtUser);
        }

        if (IsNew)
        {
            var debtId = dbUser.NextDebtId;
            dbUser.NextDebtId++; // todo обработать канкаренси

            var obj = new Debt
            {
                Id = debtId,
            };

            FillDbProperties(obj, dbDebtUser.Id);
            Environment.Context.Debts.Add(obj);
            IsNew = false;
            Environment.Context.SaveChanges();
            Id = obj.Id;
        }
        else
        {
            var obj = Environment.Context.Debts
                .IsUserEntity(User.Id, Id)
                .First();

            FillDbProperties(obj, dbDebtUser.Id);
            Environment.Context.SaveChanges();
        }
    }
}
