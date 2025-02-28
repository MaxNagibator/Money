using Money.Business.Enums;
using Money.Data.Entities;
using Money.Data.Extensions;

namespace Money.Api.Tests.TestTools.Entities;

/// <summary>
/// Авто.
/// </summary>
public class TestCarEvent : TestObject
{
    public TestCarEvent(TestUser user)
    {
        User = user;

        Title = TestRandom.GetString("CarEvent");
        TypeId = (int)TestRandom.GetEnum<CarEventTypes>();
        Comment = TestRandom.GetString("CarEventDescription");
        Mileage = TestRandom.GetInt();
        Date = DateTime.Now.Date;
    }

    /// <summary>
    /// Идентификатор.
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// Родительская категория.
    /// </summary>
    public TestCar? Car { get; set; }

    public string? Title { get; init; }

    public int TypeId { get; init; }

    public string? Comment { get; init; }

    public int? Mileage { get; init; }

    public DateTime Date { get; init; }

    /// <summary>
    /// Пользователь.
    /// </summary>
    public TestUser User { get; }

    /// <summary>
    /// Удалена.
    /// </summary>
    public bool IsDeleted { get; set; }

    public TestCarEvent SetCar(TestCar model)
    {
        Car = model;
        return this;
    }

    public TestCarEvent SetIsDeleted()
    {
        IsDeleted = true;
        return this;
    }

    private void FillDbProperties(CarEvent obj)
    {
        obj.CarId = Car!.Id;
        obj.UserId = User.Id;
        obj.Title = Title;
        obj.TypeId = TypeId;
        obj.Comment = Comment;
        obj.Mileage = Mileage;
        obj.Date = Date;
        obj.IsDeleted = IsDeleted;
    }

    public override void LocalSave()
    {
        if (IsNew)
        {
            var dbUser = Environment.Context.DomainUsers.Single(x => x.Id == User.Id);
            var carId = dbUser.NextCarEventId;
            dbUser.NextCarEventId++; // todo обработать канкаренси

            var obj = new CarEvent
            {
                Id = carId,
            };

            FillDbProperties(obj);
            Environment.Context.CarEvents.Add(obj);
            IsNew = false;
            Environment.Context.SaveChanges();
            Id = obj.Id;
        }
        else
        {
            var obj = Environment.Context.CarEvents
                .IsUserEntity(User.Id, Id)
                .First();

            FillDbProperties(obj);
            Environment.Context.SaveChanges();
        }
    }
}
