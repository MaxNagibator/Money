using Money.Business.Enums;

namespace Money.Api.Tests.TestTools.Entities;

/// <summary>
///     Категория.
/// </summary>
public class TestCategory : TestObject
{
    public TestCategory(TestUser user)
    {
        User = user;
        IsNew = true;
        Name = $"P{Guid.NewGuid()}";
        PaymentType = PaymentTypes.Costs;
    }

    /// <summary>
    ///     Идентификатор.
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    ///     Наименование.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     Родительская категория.
    /// </summary>
    public TestCategory? Parent { get; set; }

    /// <summary>
    ///     Тип.
    /// </summary>
    public PaymentTypes PaymentType { get; }

    /// <summary>
    ///     Пользователь.
    /// </summary>
    public TestUser User { get; }

    /// <summary>
    ///     Удалена.
    /// </summary>
    public bool IsDeleted { get; set; }

    public override void LocalSave()
    {
        if (IsNew)
        {
            Money.Data.Entities.DomainUser dbUser = Environment.Context.DomainUsers.Single(x => x.Id == User.Id);
            int categoryId = dbUser.NextCategoryId;
            dbUser.NextCategoryId++; // todo обработать канкаренси

            Money.Data.Entities.Category obj = new()
            {
                Id = categoryId,
                Name = "",
            };

            FillDbProperties(obj);
            Environment.Context.Categories.Add(obj);
            IsNew = false;
            Environment.Context.SaveChanges();
            Id = obj.Id;
        }
        else
        {
            Money.Data.Entities.Category obj = Environment.Context.Categories.First(x => x.UserId == User.Id && x.Id == Id);
            FillDbProperties(obj);
            Environment.Context.SaveChanges();
        }
    }

    public TestCategory SetParent(TestCategory category)
    {
        Parent = category;
        return this;
    }

    public TestCategory SetIsDeleted()
    {
        IsDeleted = true;
        return this;
    }

    public TestPayment WithPayment()
    {
        TestPayment obj = new(this);
        obj.Attach(Environment);
        return obj;
    }

    private void FillDbProperties(Money.Data.Entities.Category obj)
    {
        obj.Name = Name;
        obj.TypeId = PaymentType;
        obj.UserId = User.Id;
        obj.ParentId = Parent?.Id;
        obj.IsDeleted = IsDeleted;
    }
}
