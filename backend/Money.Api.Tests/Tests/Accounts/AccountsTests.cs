using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Money.Api.BackgroundServices;
using Money.ApiClient;
using System.Net;

namespace Money.Api.Tests.Accounts;

public class AccountsTests
{
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

        await ExecuteEmailService();

        var isEmailWithUserNameExists = TestMailsService.IsEmailWithUserNameExists(user.UserName);
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

        await ExecuteEmailService();

        var isEmailWithUserNameExists = TestMailsService.IsEmailWithUserNameExists(user.UserName);
        Assert.That(isEmailWithUserNameExists, Is.True);
    }

    /// <summary>
    ///     После регистрации с уже занятым EMAIL, нужно занулить неподтверждённый email.
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

        await ExecuteEmailService();

        _apiClient.SetUser(user);
        var code = TestMailsService.GetConfirmCode(user.UserName);
        Assert.That(code, Is.Not.Empty);
    }

    [Test]
    public async Task ConfirmEmailTest()
    {
        var user = _dbClient.WithUser();
        _dbClient.Save();

        ExecuteEmailService();

        _apiClient.SetUser(user);
        var code = TestMailsService.GetConfirmCode(user.UserName)!;
        await _apiClient.Accounts.ConfirmEmailAsync(code);

        var dbUser = await _dbClient.CreateApplicationDbContext().Users.FirstAsync(x => x.UserName == user.UserName);
        Assert.That(dbUser.EmailConfirmed, Is.True);
    }

    [Test]
    public async Task ResendConfirmCodeTest()
    {
        var user = _dbClient.WithUser();
        _dbClient.Save();

        await ExecuteEmailService();

        _apiClient.SetUser(user);
        await _apiClient.Accounts.ResendConfirmCodeAsync();

        await ExecuteEmailService();

        var codes = TestMailsService
            .GetEmailsByUserName(user.UserName)
            .Select(TestMailsService.GetConfirmCode)
            .ToArray();

        Assert.That(codes, Is.Not.Empty);
        Assert.That(codes, Has.Length.GreaterThanOrEqualTo(2));
        Assert.That(codes, Is.Unique);
    }

    [Test]
    public async Task ChangePasswordTest()
    {
        var user = _dbClient.WithUser();
        _dbClient.Save();

        _apiClient.SetUser(user);
        var newPassword = user.Password + "_upd";
        await _apiClient.Accounts.ChangePassword(user.Password, newPassword).IsSuccess();

        _apiClient.User.AuthData = null;
        var exFound = false;
        try
        {
            await _apiClient.Cars.Get();
        }
        catch (HttpRequestException ex)
        {
            if (ex.Message.Contains("403"))
            {
                exFound = true;
            }
            else
            {
                throw;
            }
        }
        Assert.That(exFound, Is.True);
        user.SetPassword(newPassword);
        _apiClient.SetUser(user);
        var result = await _apiClient.Cars.Get();
        Assert.That(result.Code, Is.EqualTo(HttpStatusCode.OK));
    }

    private static Task ExecuteEmailService()
    {
        using var scope = Integration.ServiceProvider.CreateScope();

        var sender = scope.ServiceProvider.GetServices<IHostedService>()
            .OfType<EmailSenderBackgroundService>()
            .Single();

        return sender.ForceExecuteAsync(CancellationToken.None);
    }
}
