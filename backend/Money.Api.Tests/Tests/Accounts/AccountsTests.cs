using Microsoft.EntityFrameworkCore;

namespace Money.Api.Tests.Accounts;

public class AccountsTests
{
    private DatabaseClient _dbClient;

    [SetUp]
    public void Setup()
    {
        _dbClient = Integration.GetDatabaseClient();
    }

    [Test]
    public async Task RegisterWithoutEmailTest()
    {
        var user = _dbClient.WithUser().SetEmail(null);
        _dbClient.Save();

        var dbUser = await _dbClient.CreateApplicationDbContext()
            .Users
            .FirstOrDefaultAsync(x => x.UserName == user.UserName);

        Assert.That(dbUser, Is.Not.Null);
        Assert.That(dbUser.Email, Is.Null);

        var isEmailWithUserNameExists = TestMailService.IsEmailWithUserNameExists(user);
        Assert.That(isEmailWithUserNameExists, Is.False);
    }

    [Test]
    public async Task RegisterWithEmailTest()
    {
        var user = _dbClient.WithUser();
        _dbClient.Save();

        var dbUser = await _dbClient.CreateApplicationDbContext()
            .Users
            .FirstOrDefaultAsync(x => x.UserName == user.UserName);

        Assert.That(dbUser, Is.Not.Null);
        Assert.That(dbUser.Email, Is.EqualTo(user.Email));

        await Task.Delay(10001); // ждём по максимум бэкграунд сервис

        var isEmailWithUserNameExists = TestMailService.IsEmailWithUserNameExists(user);
        Assert.That(isEmailWithUserNameExists, Is.True);
    }

    /// <summary>
    /// После регистрации с уже занятым EMAIL, нужно занулить неподтверждённый email.
    /// </summary>
    /// <returns></returns>
    [Test]
    public async Task RegisterWithSameEmailTest()
    {
        var user1 = _dbClient.WithUser();
        var user2 = _dbClient.WithUser().SetEmail(user1.Email);
        _dbClient.Save();

        await using var context = _dbClient.CreateApplicationDbContext();

        var dbUser1 = await context.Users.FirstOrDefaultAsync(x => x.UserName == user1.UserName);
        var dbUser2 = await context.Users.FirstOrDefaultAsync(x => x.UserName == user2.UserName);

        Assert.Multiple(() =>
        {
            Assert.That(dbUser1, Is.Not.Null);
            Assert.That(dbUser2, Is.Not.Null);
        });

        Assert.Multiple(() =>
        {
            Assert.That(dbUser1.Email, Is.Null);
            Assert.That(dbUser2.Email, Is.EqualTo(user2.Email));
        });
    }
}
