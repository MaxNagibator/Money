namespace Money.Api.Tests.TestTools.Entities;

/// <summary>
///     Пользователь.
/// </summary>
public class TestUser : TestObject
{
    public TestUser()
    {
        IsNew = true;
        Login = $"test_{Guid.NewGuid()}@bobgroup.test.ru";
        Password = "123Qwerty9000!";
    }

    /// <summary>
    ///     Идентификатор.
    /// </summary>
    public int Id { get; private set; }

    public string Login { get; }
    public string Password { get; }

    public override void LocalSave()
    {
        if (IsNew)
        {
            Integration.Register(Login, Password).Wait();
            Money.Data.Entities.ApplicationUser dbUser = Environment.Context.Users.Single(x => x.UserName == Login);
            Money.Data.Entities.DomainUser domainUser = Environment.Context.DomainUsers.Single(x => x.AuthUserId == dbUser.Id);
            Id = domainUser.Id;
        }
    }

    public TestCategory WithCategory()
    {
        TestCategory obj = new(this);
        obj.Attach(Environment);
        return obj;
    }
}
