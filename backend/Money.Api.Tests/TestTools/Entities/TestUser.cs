namespace Money.Api.Tests.TestTools.Entities;

/// <summary>
/// Пользователь.
/// </summary>
public class TestUser : TestObject
{
    public TestUser()
    {
        UserName = $"test_{Guid.NewGuid()}";
        Email = $"{UserName}@bobgroup.test.ru";
        Password = "123Qwerty9000!";
    }

    /// <summary>
    /// Идентификатор.
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// Логин.
    /// </summary>
    public string UserName { get; private set; }

    /// <summary>
    /// Email.
    /// </summary>
    public string? Email { get; private set; }

    /// <summary>
    /// Пароль.
    /// </summary>
    public string Password { get; }

    public TestUser SetUserName(string value)
    {
        UserName = value;
        return this;
    }

    public TestUser SetEmail(string? value)
    {
        Email = value;
        return this;
    }

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
            Environment.ApiClient.RegisterAsync(UserName, Email, Password).Wait();

            var dbUser = Environment.Context.Users
                .Single(x => x.UserName == UserName);

            var domainUser = Environment.Context.DomainUsers
                .Single(x => x.AuthUserId == dbUser.Id);

            Id = domainUser.Id;
        }
    }
}
