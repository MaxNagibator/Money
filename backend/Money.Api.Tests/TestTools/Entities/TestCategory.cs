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
    ///     Тип.
    /// </summary>
    public PaymentTypes PaymentType { get; }

    /// <summary>
    ///     Пользователь.
    /// </summary>
    public TestUser User { get; }

    public override void LocalSave()
    {
        if (IsNew)
        {
            //todo need optimization in future
            int categoryId = Environment.Context.Categories.AsEnumerable()
                                 .Where(x => x.UserId == User.Id)
                                 .Select(x => x.Id)
                                 .DefaultIfEmpty(0)
                                 .Max()
                             + 1;

            Money.Data.Entities.Category obj = new()
            {
                Id = categoryId,
                Name = ""
            };

            FillDbProperties(obj);
            Environment.Context.Categories.Add(obj);
            IsNew = false;
            Environment.Context.SaveChanges();
            Id = obj.Id;
        }
        else
        {
            Money.Data.Entities.Category obj = Environment.Context.Categories.First(x => x.Id == Id);
            FillDbProperties(obj);
            Environment.Context.SaveChanges();
        }
    }

    private void FillDbProperties(Money.Data.Entities.Category obj)
    {
        obj.Name = Name;
        obj.TypeId = PaymentType;
        obj.UserId = User.Id;
    }
}
