using Money.Data.Entities;

namespace Money.Api.Tests.TestTools.Entities;

public class TestPlace : TestObject
{
    public TestPlace(TestUser user)
    {
        User = user;
        Name = "Place_" + Guid.NewGuid();
        IsNew = true;
    }

    public int Id { get; private set; }

    public string Name { get; }

    /// <summary>
    ///     Пользователь.
    /// </summary>
    public TestUser User { get; }

    private void FillDbProperties(DomainPlace obj)
    {
        obj.Name = Name;
    }

    public override void LocalSave()
    {
        if (IsNew)
        {
            DomainUser dbUser = Environment.Context.DomainUsers.Single(x => x.Id == User.Id);
            int id = dbUser.NextPlaceId;
            dbUser.NextPlaceId++; // todo обработать канкаренси

            DomainPlace obj = new()
            {
                Id = id,
                Name = "",
                UserId = User.Id,
            };

            FillDbProperties(obj);
            Environment.Context.Places.Add(obj);
            IsNew = false;
            Environment.Context.SaveChanges();
            Id = obj.Id;
        }
        else
        {
            DomainPlace obj = Environment.Context.Places.First(x => x.UserId == User.Id && x.Id == Id);
            FillDbProperties(obj);
            Environment.Context.SaveChanges();
        }
    }
}
