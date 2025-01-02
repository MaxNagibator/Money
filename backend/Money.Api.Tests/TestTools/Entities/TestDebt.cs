using Money.Business.Models;

namespace Money.Api.Tests.TestTools.Entities;

/// <summary>
///     Долг.
/// </summary>
public class TestDebt : TestObject
{
    public TestDebt(TestUser user)
    {
        User = user;
        IsNew = true;
        Type = DebtTypes.Plus;
        Sum = 217;
        Comment = "Comment" + Guid.NewGuid().ToString();
        Date = DateTime.Now.Date;
        PaySum = 0;
        Status = DebtStatus.Actual;
        DebtUserName = "U" + Guid.NewGuid().ToString();
    }

    /// <summary>
    ///     Пользователь.
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
    ///     Удалена.
    /// </summary>
    public bool IsDeleted { get; set; }

    private void FillDbProperties(Data.Entities.Debt dbDebt, int debtUserId)
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
        Data.Entities.DomainUser dbUser = Environment.Context.DomainUsers.Single(x => x.Id == User.Id);

        Data.Entities.DebtUser? dbDebtUser = Environment.Context.DebtUsers
            .SingleOrDefault(x => x.UserId == User.Id && x.Name == DebtUserName);
        if (dbDebtUser == null)
        {
            var debtUserId = dbUser.NextDebtUserId;
            dbUser.NextDebtUserId++;

            dbDebtUser = new Data.Entities.DebtUser
            {
                Name = DebtUserName
            };
            dbDebtUser.UserId = User.Id;
            dbDebtUser.Id = debtUserId;
            Environment.Context.DebtUsers.Add(dbDebtUser);
        }

        if (IsNew)
        {
            int debtId = dbUser.NextDebtId;
            dbUser.NextDebtId++; // todo обработать канкаренси

            Data.Entities.Debt obj = new()
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
            Data.Entities.Debt obj = Environment.Context.Debts.First(x => x.UserId == User.Id && x.Id == Id);
            FillDbProperties(obj, dbDebtUser.Id);
            Environment.Context.SaveChanges();
        }
    }
}
