using Money.Data.Entities;
using Money.Data.Extensions;

namespace Money.Api.Tests.TestTools.Entities;

public class TestPlace : TestObject
{
    public TestPlace(TestUser user)
    {
        User = user;

        Name = TestRandom.GetString("Place");
    }

    public int Id { get; private set; }

    public string Name { get; }

    public DateTime LastUsedDate { get; private set; }

    /// <summary>
    /// Пользователь.
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
            var dbUser = Environment.Context.DomainUsers.Single(x => x.Id == User.Id);
            var id = dbUser.NextPlaceId;
            dbUser.NextPlaceId++; // todo обработать канкаренси

            var obj = new Place
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
            var obj = Environment.Context.Places
                .IsUserEntity(User.Id, Id)
                .First();

            FillDbProperties(obj);
            Environment.Context.SaveChanges();
        }
    }
}
