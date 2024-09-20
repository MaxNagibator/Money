using Money.Business.Enums;
using Polly;
using System;

namespace Money.Api.Tests.TestTools;

/// <summary>
/// Категория.
/// </summary>
public class TestCategory : TestObject
{
    /// <summary>
    /// Идентификатор.
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// Наименование.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Тип.
    /// </summary>
    public PaymentTypes PaymentType { get; set; }

    /// <summary>
    /// Пользователь.
    /// </summary>
    public TestUser User { get; set; }

    public TestCategory(TestUser user)
    {
        IsNew = true;
        Name = "P" + Guid.NewGuid();
        PaymentType = PaymentTypes.Costs;
        User = user;
    }

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

            var obj = new Money.Data.Entities.Category
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
            var obj = Environment.Context.Categories.First(x => x.Id == Id);
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
