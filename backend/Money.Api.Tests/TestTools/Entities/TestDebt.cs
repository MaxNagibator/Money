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
        Sum = TestRandom.GetInt(minValue: 1);
        Comment = TestRandom.GetString("Comment");
        PayComment = TestRandom.GetString("PayComment");
        OwnerName = TestRandom.GetString("User");

        Date = DateTime.Now.Date;
        Status = DebtStatus.Actual;
        PaySum = 0;
    }

    /// <summary>
    /// Пользователь.
    /// </summary>
    public TestUser User { get; }

    public int Id { get; private set; }

    public DebtTypes Type { get; private set; }

    public decimal Sum { get; private set; }

    public string? Comment { get; }

    public string OwnerName { get; private set; }

    public int OwnerId { get; private set; }

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
    public TestDebt SetSum(decimal value)
    {
        Sum = value;
        return this;
    }

    public TestDebt SetOwnerName(string value)
    {
        OwnerName = value;
        return this;
    }

    public TestDebt SetType(DebtTypes value)
    {
        Type = value;
        return this;
    }

    private void FillDbProperties(Debt dbDebt, int ownerId)
    {
        dbDebt.Sum = Sum;
        dbDebt.TypeId = (int)Type;
        dbDebt.OwnerId = ownerId;
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

        var dbOwner = Environment.Context.DebtOwners
            .SingleOrDefault(x => x.UserId == User.Id && x.Name == OwnerName);

        if (dbOwner == null)
        {
            var debtOwnerId = dbUser.NextDebtOwnerId;
            dbUser.NextDebtOwnerId++;

            dbOwner = new()
            {
                Name = OwnerName,
                UserId = User.Id,
                Id = debtOwnerId,
            };

            Environment.Context.DebtOwners.Add(dbOwner);
        }

        OwnerId = dbOwner.Id;

        if (IsNew)
        {
            var debtId = dbUser.NextDebtId;
            dbUser.NextDebtId++; // todo обработать канкаренси

            var obj = new Debt
            {
                Id = debtId,
            };

            FillDbProperties(obj, dbOwner.Id);
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

            FillDbProperties(obj, dbOwner.Id);
            Environment.Context.SaveChanges();
        }
    }
}
