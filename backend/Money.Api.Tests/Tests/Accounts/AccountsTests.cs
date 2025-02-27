using Microsoft.EntityFrameworkCore;
using Money.Api.BackgroundServices;
using Money.ApiClient;

namespace Money.Api.Tests.Accounts;

public class AccountsTests
{
    private readonly TimeSpan _emailServiceDelay = EmailSenderBackgroundService.Delay + TimeSpan.FromMilliseconds(100);

    private DatabaseClient _dbClient;
    private MoneyClient _apiClient;

    [SetUp]
    public void Setup()
    {
        _dbClient = Integration.GetDatabaseClient();
        _apiClient = new(Integration.GetHttpClient(), Console.WriteLine);
    }

    [Test]
    public void RegisterWithAvailableUserNameTest()
    {
        _dbClient.WithUser().SetUserName(TestRandom.GetString("test"));

        Assert.DoesNotThrow(() => _dbClient.Save());
    }

    [Test]
    public void RegisterWithUnavailableUserNameTest()
    {
        _dbClient.WithUser().SetUserName("bob217@");

        Assert.Throws<AggregateException>(() => _dbClient.Save());
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

        var isEmailWithUserNameExists = TestMailService.IsEmailWithUserNameExists(user.UserName);
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

        await Task.Delay(_emailServiceDelay); // ждём по максимум бэкграунд сервис

        var isEmailWithUserNameExists = TestMailService.IsEmailWithUserNameExists(user.UserName);
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

    [Test]
    public async Task ExistConfirmCodeTest()
    {
        var user = _dbClient.WithUser();
        _dbClient.Save();
        await Task.Delay(_emailServiceDelay); // ждём по максимум бэкграунд сервис // todo это шляпа

        _apiClient.SetUser(user);
        var code = TestMailService.GetConfirmCode(user.UserName);
        Assert.That(code, Is.Not.Empty);
    }

    [Test]
    public async Task ConfirmEmailTest()
    {
        var user = _dbClient.WithUser();
        _dbClient.Save();
        await Task.Delay(_emailServiceDelay); // ждём по максимум бэкграунд сервис // todo это шляпа

        _apiClient.SetUser(user);
        var code = TestMailService.GetConfirmCode(user.UserName)!;
        await _apiClient.Accounts.ConfirmEmailAsync(code);

        var dbUser = await _dbClient.CreateApplicationDbContext().Users.FirstAsync(x => x.UserName == user.UserName);
        Assert.That(dbUser.EmailConfirmed, Is.True);
    }

    [Test]
    [Ignore("Нестабильное поведение")]
    public async Task ResendConfirmCodeTest()
    {
        var user = _dbClient.WithUser();
        _dbClient.Save();
        await Task.Delay(_emailServiceDelay); // ждём по максимум бэкграунд сервис // todo это шляпа

        _apiClient.SetUser(user);
        await _apiClient.Accounts.ResendConfirmCodeAsync();
        await Task.Delay(_emailServiceDelay); // ждём по максимум бэкграунд сервис // todo это шляпа

        var codes = TestMailService
            .GetEmailsByUserName(user.UserName)
            .Select(TestMailService.GetConfirmCode)
            .ToArray();

        Assert.That(codes, Is.Not.Empty);
        Assert.That(codes, Has.Length.GreaterThanOrEqualTo(2));
        Assert.That(codes, Is.Unique);
    }
}
