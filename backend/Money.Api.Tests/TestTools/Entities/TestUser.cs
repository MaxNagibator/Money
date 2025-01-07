namespace Money.Api.Tests.TestTools.Entities;

/// <summary>
/// Пользователь.
/// </summary>
public class TestUser : TestObject
{
    public TestUser()
    {
        Login = $"test_{Guid.NewGuid()}@bobgroup.test.ru";
        Password = "123Qwerty9000!";
    }

    /// <summary>
    /// Идентификатор.
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// Логин.
    /// </summary>
    public string Login { get; }

    /// <summary>
    /// Пароль.
    /// </summary>
    public string Password { get; }

    public TestCategory WithCategory()
    {
        var obj = new TestCategory(this);
        obj.Attach(Environment);
        return obj;
    }

    public TestOperation WithOperation()
    {
        var obj = new TestOperation(WithCategory());
        obj.Attach(Environment);
        return obj;
    }

    public TestFastOperation WithFastOperation()
    {
        var obj = new TestFastOperation(WithCategory());
        obj.Attach(Environment);
        return obj;
    }

    public TestRegularOperation WithRegularOperation()
    {
        var obj = new TestRegularOperation(WithCategory());
        obj.Attach(Environment);
        return obj;
    }

    public TestPlace WithPlace()
    {
        var obj = new TestPlace(this);
        obj.Attach(Environment);
        return obj;
    }

    public TestDebt WithDebt()
    {
        var obj = new TestDebt(this);
        obj.Attach(Environment);
        return obj;
    }

    public override void LocalSave()
    {
        if (IsNew)
        {
            Environment.ApiClient.RegisterAsync(Login, Password).Wait();

            var dbUser = Environment.Context.Users
                .Single(x => x.UserName == Login);

            var domainUser = Environment.Context.DomainUsers
                .Single(x => x.AuthUserId == dbUser.Id);

            Id = domainUser.Id;
        }
    }
}
