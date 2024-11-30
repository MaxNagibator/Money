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

    public DateTime LastUsedDate { get; private set; }

    /// <summary>
    ///     Пользователь.
    /// </summary>
    public TestUser User { get; }

    public TestPlace SetLastUsedDate(DateTime value)
    {
        LastUsedDate = value;
        return this;
    }

    private void FillDbProperties(Place obj)
    {
        obj.Name = Name;
        obj.LastUsedDate = LastUsedDate;
    }

    public override void LocalSave()
    {
        if (IsNew)
        {
            DomainUser dbUser = Environment.Context.DomainUsers.Single(x => x.Id == User.Id);
            int id = dbUser.NextPlaceId;
            dbUser.NextPlaceId++; // todo обработать канкаренси

            Place obj = new()
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
            Place obj = Environment.Context.Places.First(x => x.UserId == User.Id && x.Id == Id);
            FillDbProperties(obj);
            Environment.Context.SaveChanges();
        }
    }
}
