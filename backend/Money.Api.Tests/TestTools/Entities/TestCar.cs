using Money.Data.Entities;
using Money.Data.Extensions;

namespace Money.Api.Tests.TestTools.Entities;

/// <summary>
/// Авто.
/// </summary>
public class TestCar : TestObject
{
    public TestCar(TestUser user)
    {
        User = user;

        Name = TestRandom.GetString("Car");
    }

    /// <summary>
    /// Идентификатор.
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// Наименование.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Пользователь.
    /// </summary>
    public TestUser User { get; }

    /// <summary>
    /// Удалена.
    /// </summary>
    public bool IsDeleted { get; set; }

    public TestCar SetIsDeleted()
    {
        IsDeleted = true;
        return this;
    }

    public TestCarEvent WithEvent()
    {
        var obj = new TestCarEvent(User, this);
        obj.Attach(Environment);
        return obj;
    }

    private void FillDbProperties(Car obj)
    {
        obj.UserId = User.Id;
        obj.Name = Name;
        obj.IsDeleted = IsDeleted;
    }

    public override void LocalSave()
    {
        if (IsNew)
        {
            var dbUser = Environment.Context.DomainUsers.Single(x => x.Id == User.Id);
            var carId = dbUser.NextCarId;
            dbUser.NextCarId++; // todo обработать канкаренси

            var obj = new Car
            {
                Id = carId,
                Name = "",
            };

            FillDbProperties(obj);
            Environment.Context.Cars.Add(obj);
            IsNew = false;
            Environment.Context.SaveChanges();
            Id = obj.Id;
        }
        else
        {
            var obj = Environment.Context.Cars
                .IsUserEntity(User.Id, Id)
                .First();

            FillDbProperties(obj);
            Environment.Context.SaveChanges();
        }
    }
}
