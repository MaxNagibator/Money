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
}
