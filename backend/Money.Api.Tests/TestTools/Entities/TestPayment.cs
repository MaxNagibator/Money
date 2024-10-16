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
        Date = DateTime.Now.Date;
    }

    /// <summary>
    ///     Идентификатор.
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    ///     Сумма.
    /// </summary>
    public decimal Sum { get; private set; }

    /// <summary>
    ///     Комментарий.
    /// </summary>
    public string Comment { get; private set; }

    /// <summary>
    ///     Дата.
    /// </summary>
    public DateTime Date { get; private set; }

    /// <summary>
    ///     Категория.
    /// </summary>
    public TestCategory Category { get; }

    /// <summary>
    ///     Пользователь.
    /// </summary>
    public TestUser User => Category.User;

    /// <summary>
    ///     Место.
    /// </summary>
    public TestPlace Place { get; private set; } = null!;

    /// <summary>
    ///     Удалена.
    /// </summary>
    public bool IsDeleted { get; private set; }

    public override void LocalSave()
    {
        if (IsNew)
        {
            Data.Entities.DomainUser dbUser = Environment.Context.DomainUsers.Single(x => x.Id == User.Id);
            int paymentId = dbUser.NextPaymentId;
            dbUser.NextPaymentId++; // todo обработать канкаренси

            Data.Entities.Payment obj = new()
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
            Data.Entities.Payment obj = Environment.Context.Payments.First(x => x.UserId == User.Id && x.Id == Id);
            FillDbProperties(obj);
            Environment.Context.SaveChanges();
        }
    }

    public TestPayment SetIsDeleted()
    {
        IsDeleted = true;
        return this;
    }

    public TestPayment SetDate(DateTime value)
    {
        Date = value;
        return this;
    }

    public TestPayment SetPlace(TestPlace value)
    {
        Place = value;
        return this;
    }

    public TestPayment SetComment(string value)
    {
        Comment = value;
        return this;
    }

    public TestPayment SetSum(decimal value)
    {
        Sum = value;
        return this;
    }

    private void FillDbProperties(Data.Entities.Payment obj)
    {
        obj.Comment = Comment;
        obj.Sum = Sum;
        obj.UserId = User.Id;
        obj.CategoryId = Category.Id;
        obj.IsDeleted = IsDeleted;
        obj.Date = Date;
        obj.PlaceId = Place?.Id;
    }
}
