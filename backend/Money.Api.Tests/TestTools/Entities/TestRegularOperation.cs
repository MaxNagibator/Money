using Money.Business.Enums;
using Money.Data.Entities;
using Money.Data.Extensions;

namespace Money.Api.Tests.TestTools.Entities;

/// <summary>
/// Регулярная операция.
/// </summary>
public class TestRegularOperation : TestObject
{
    public TestRegularOperation(TestCategory category)
    {
        Category = category;

        Sum = TestRandom.GetInt();

        Name = TestRandom.GetString("RegularOperation");
        Comment = TestRandom.GetString("RegularOperation");

        DateFrom = DateTime.Now.Date;
        DateTo = DateTime.Now.Date;
        RunTime = DateTime.Now.Date;
        TimeType = RegularOperationTimeTypes.EveryMonth;
        TimeValue = 1;
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
    public TestPlace? Place { get; private set; }

    public RegularOperationTimeTypes TimeType { get; }

    public int? TimeValue { get; }

    public DateTime DateFrom { get; }

    public DateTime? DateTo { get; }

    public DateTime? RunTime { get; }

    /// <summary>
    /// Удалена.
    /// </summary>
    public bool IsDeleted { get; private set; }

    public TestRegularOperation SetIsDeleted()
    {
        IsDeleted = true;
        return this;
    }

    public TestRegularOperation SetName(string value)
    {
        Name = value;
        return this;
    }

    public TestRegularOperation SetPlace(TestPlace value)
    {
        Place = value;
        return this;
    }

    public TestRegularOperation SetComment(string value)
    {
        Comment = value;
        return this;
    }

    public TestRegularOperation SetSum(decimal value)
    {
        Sum = value;
        return this;
    }

    private void FillDbProperties(RegularOperation obj)
    {
        obj.Name = Name;
        obj.Comment = Comment;
        obj.Sum = Sum;
        obj.UserId = User.Id;
        obj.CategoryId = Category.Id;
        obj.IsDeleted = IsDeleted;
        obj.PlaceId = Place?.Id;
        obj.TimeValue = TimeValue;
        obj.DateFrom = DateFrom;
        obj.DateTo = DateTo;
        obj.RunTime = RunTime;
        obj.TimeTypeId = (int)TimeType;
    }

    public override void LocalSave()
    {
        if (IsNew)
        {
            var dbUser = Environment.Context.DomainUsers.Single(x => x.Id == User.Id);
            var operationId = dbUser.NextRegularOperationId;
            dbUser.NextRegularOperationId++; // todo обработать канкаренси

            var obj = new RegularOperation
            {
                Id = operationId,
                Name = "new",
            };

            FillDbProperties(obj);
            Environment.Context.RegularOperations.Add(obj);
            IsNew = false;
            Environment.Context.SaveChanges();
            Id = obj.Id;
        }
        else
        {
            var obj = Environment.Context.RegularOperations
                .IsUserEntity(User.Id, Id)
                .First();

            FillDbProperties(obj);
            Environment.Context.SaveChanges();
        }
    }
}
