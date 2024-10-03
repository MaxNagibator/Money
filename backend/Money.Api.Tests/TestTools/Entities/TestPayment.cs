namespace Money.Api.Tests.TestTools.Entities;

/// <summary>
///     Платеж.
/// </summary>
public class TestPayment : TestObject
{
    public TestPayment(TestCategory category)
    {
        IsNew = true;
        Sum = 100;
        Comment = $"P{Guid.NewGuid()}";
        Category = category;
    }

    /// <summary>
    ///     Идентификатор.
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    ///     Сумма.
    /// </summary>
    public decimal Sum { get; set; }

    /// <summary>
    ///     Комментарий.
    /// </summary>
    public string Comment { get; }

    /// <summary>
    ///     Категория.
    /// </summary>
    public TestCategory Category { get; set; }

    /// <summary>
    ///     Пользователь.
    /// </summary>
    public TestUser User => Category.User;

    /// <summary>
    ///     Удалена.
    /// </summary>
    public bool IsDeleted { get; set; }

    public override void LocalSave()
    {
        if (IsNew)
        {
            Money.Data.Entities.DomainUser dbUser = Environment.Context.DomainUsers.Single(x => x.Id == User.Id);
            int paymentId = dbUser.NextPaymentId;
            dbUser.NextPaymentId++; // todo обработать канкаренси

            Money.Data.Entities.Payment obj = new()
            {
                Id = paymentId,
            };

            FillDbProperties(obj);
            Environment.Context.Payments.Add(obj);
            IsNew = false;
            Environment.Context.SaveChanges();
            Id = obj.Id;
        }
        else
        {
            Money.Data.Entities.Payment obj = Environment.Context.Payments.First(x => x.Id == Id);
            FillDbProperties(obj);
            Environment.Context.SaveChanges();
        }
    }

    public TestPayment SetIsDeleted()
    {
        IsDeleted = true;
        return this;
    }

    private void FillDbProperties(Money.Data.Entities.Payment obj)
    {
        obj.Comment = Comment;
        obj.Sum = Sum;
        obj.UserId = User.Id;
        obj.CategoryId = obj.CategoryId;
        obj.IsDeleted = IsDeleted;
    }
}
