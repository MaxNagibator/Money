using Money.Data.Entities;
using Money.Data.Extensions;

namespace Money.Api.Tests.TestTools.Entities;

/// <summary>
/// Быстрая операция.
/// </summary>
public class TestFastOperation : TestObject
{
    public TestFastOperation(TestCategory category)
    {
        Category = category;

        Sum = TestRandom.GetInt();
        Name = TestRandom.GetString("FastOperation");
        Comment = TestRandom.GetString("FastOperation");
    }

    /// <summary>
    /// Идентификатор.
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// Сумма.
    /// </summary>
    public decimal Sum { get; private set; }

    /// <summary>
    /// Наименование.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Порядок сортировки.
    /// </summary>
    public int? Order { get; private set; }

    /// <summary>
    /// Комментарий.
    /// </summary>
    public string Comment { get; private set; }

    /// <summary>
    /// Категория.
    /// </summary>
    public TestCategory Category { get; }

    /// <summary>
    /// Пользователь.
    /// </summary>
    public TestUser User => Category.User;

    /// <summary>
    /// Место.
    /// </summary>
    public TestPlace Place { get; private set; } = null!;

    /// <summary>
    /// Удалена.
    /// </summary>
    public bool IsDeleted { get; private set; }

    public TestFastOperation SetIsDeleted()
    {
        IsDeleted = true;
        return this;
    }

    public TestFastOperation SetName(string value)
    {
        Name = value;
        return this;
    }

    public TestFastOperation SetPlace(TestPlace value)
    {
        Place = value;
        return this;
    }

    public TestFastOperation SetComment(string value)
    {
        Comment = value;
        return this;
    }

    public TestFastOperation SetSum(decimal value)
    {
        Sum = value;
        return this;
    }

    public TestFastOperation SetOrder(int value)
    {
        Order = value;
        return this;
    }

    private void FillDbProperties(FastOperation obj)
    {
        obj.Name = Name;
        obj.Order = Order;
        obj.Comment = Comment;
        obj.Sum = Sum;
        obj.UserId = User.Id;
        obj.CategoryId = Category.Id;
        obj.IsDeleted = IsDeleted;
        obj.PlaceId = Place?.Id;
    }

    public override void LocalSave()
    {
        if (IsNew)
        {
            var dbUser = Environment.Context.DomainUsers.Single(x => x.Id == User.Id);
            var operationId = dbUser.NextFastOperationId;
            dbUser.NextFastOperationId++; // todo обработать канкаренси

            var obj = new FastOperation
            {
                Id = operationId,
                Name = "new",
            };

            FillDbProperties(obj);
            Environment.Context.FastOperations.Add(obj);
            IsNew = false;
            Environment.Context.SaveChanges();
            Id = obj.Id;
        }
        else
        {
            var obj = Environment.Context.FastOperations
                .IsUserEntity(User.Id, Id)
                .First();

            FillDbProperties(obj);
            Environment.Context.SaveChanges();
        }
    }
}
