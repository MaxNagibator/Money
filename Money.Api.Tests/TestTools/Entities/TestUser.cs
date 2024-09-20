namespace Money.Api.Tests.TestTools;

/// <summary>
/// Пользователь.
/// </summary>
public class TestUser : TestObject
{
    /// <summary>
    /// Идентификатор.
    /// </summary>
    public int Id { get; private set; }
    public string Login { get; }
    public string Pasword { get; }

    public TestUser()
    {
        IsNew = true;
        Login = "test_" + Guid.NewGuid() + "@bobgroup.test.ru";
        Pasword = "123Qwerty9000!";
    }

    public TestCategory WithCategory()
    {
        var obj = new TestCategory(this);
        obj.Attach(Environment);
        return obj;
    }

    public override void LocalSave()
    {
        if (IsNew)
        {
            Integration.Register(Login, Pasword).Wait();
            var dbUser = Environment.Context.Users.Single(x => x.UserName == Login);
            var domainUser = Environment.Context.DomainUsers.Single(x => x.AuthUserId == dbUser.Id);
            Id = domainUser.Id;
        }
    }
}
