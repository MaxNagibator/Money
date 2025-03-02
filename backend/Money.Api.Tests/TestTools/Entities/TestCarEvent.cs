using Money.Business.Enums;
using Money.Data.Entities;
using Money.Data.Extensions;

namespace Money.Api.Tests.TestTools.Entities;

/// <summary>
/// Авто-событие.
/// </summary>
public class TestCarEvent : TestObject
{
    public TestCarEvent(TestUser user, TestCar car)
    {
        User = user;
        Car = car;

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
    /// Связанный автомобиль.
    /// </summary>
    public TestCar Car { get; }

    /// <summary>
    /// Название.
    /// </summary>
    public string? Title { get; }

    /// <summary>
    /// Идентификатор типа.
    /// </summary>
    public int TypeId { get; }

    /// <summary>
    /// Дополнительные комментарии.
    /// </summary>
    public string? Comment { get; }

    /// <summary>
    /// Пробег автомобиля.
    /// </summary>
    public int? Mileage { get; }

    /// <summary>
    /// Дата.
    /// </summary>
    public DateTime Date { get; }

    /// <summary>
    /// Пользователь.
    /// </summary>
    public TestUser User { get; }

    /// <summary>
    /// Удалено.
    /// </summary>
    public bool IsDeleted { get; set; }

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
