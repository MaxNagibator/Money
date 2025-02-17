using Microsoft.EntityFrameworkCore;
using Money.Data.Extensions;

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

        var dbUser = await _dbClient.CreateApplicationDbContext().Users.FirstOrDefaultAsync(x => x.UserName == user.Login);
        Assert.That(dbUser, Is.Not.Null);
        Assert.That(dbUser.Email, Is.Null);
    }

    [Test]
    public async Task RegisterWithEmailTest()
    {
        var user = _dbClient.WithUser();
        _dbClient.Save();

        var dbUser = await _dbClient.CreateApplicationDbContext().Users.FirstOrDefaultAsync(x => x.UserName == user.Login);
        Assert.That(dbUser, Is.Not.Null);
        Assert.That(dbUser.Email, Is.EqualTo(user.Email));
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

        var dbUser1 = await _dbClient.CreateApplicationDbContext().Users.FirstAsync(x => x.UserName == user1.Login);
        var dbUser2 = await _dbClient.CreateApplicationDbContext().Users.FirstAsync(x => x.UserName == user2.Login);
        Assert.That(dbUser1.Email, Is.Null);
        Assert.That(dbUser2.Email, Is.EqualTo(user2.Email));
    }
}
