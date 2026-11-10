using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Money.Api.BackgroundServices;
using Money.ApiClient;
using Money.Business;
using Money.Business.Configs;
using Money.Business.Services;
using Money.Common.Exceptions;
using Money.Data;
using Money.Data.Entities;
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

        var secondResend = await _apiClient.Accounts.ResendConfirmCodeAsync();
        Assert.That(secondResend.Code, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task ExternalLoginRejectsUntrustedReturnUrlTest()
    {
        using var client = Integration.GetHttpClient();
        var uri = new Uri("/external/login/github?returnUrl=https://evil.example/callback", UriKind.Relative);
        using var response = await client.GetAsync(uri);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task SensitiveAuthRateLimitTest()
    {
        using var client = Integration.GetHttpClient();
        client.DefaultRequestHeaders.Add("X-Forwarded-For", "203.0.113.77");
        var uri = new Uri("/external/login/github?returnUrl=https://evil.example/callback", UriKind.Relative);

        var lastStatus = HttpStatusCode.OK;
        for (var i = 0; i < 21; i++)
        {
            using var response = await client.GetAsync(uri);
            lastStatus = response.StatusCode;
        }

        Assert.That(lastStatus, Is.EqualTo(HttpStatusCode.TooManyRequests));

        using var passwordContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["username"] = "user",
            ["password"] = "password",
        });

        using var passwordResponse = await client.PostAsync(new Uri("/connect/token", UriKind.Relative), passwordContent);
        Assert.That(passwordResponse.StatusCode, Is.EqualTo(HttpStatusCode.TooManyRequests));

        using var refreshContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = "invalid-token",
        });

        using var refreshResponse = await client.PostAsync(new Uri("/connect/token", UriKind.Relative), refreshContent);
        Assert.That(refreshResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task ConfirmEmailLockoutAndResendCooldownUnitTest()
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "unit_user",
            Email = "unit@test.ru",
            EmailConfirmed = false,
            EmailConfirmCode = "654321",
        };

        using var store = new SingleUserStore(user);
        using var userManager = new UserManager<ApplicationUser>(store,
            Options.Create(new IdentityOptions()),
            new PasswordHasher<ApplicationUser>(),
            [],
            [],
            new UpperInvariantLookupNormalizer(),
            new(),
            null!,
            NullLogger<UserManager<ApplicationUser>>.Instance);

        using var cache = new MemoryCache(new MemoryCacheOptions());
        var env = new RequestEnvironment { AuthUser = user };
        var queueHolder = new QueueHolder();
        var saveGate = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        var saveshouldFail = false;

        await using var context = new StubDbContext(async _ =>
        {
            if (saveShouldFail())
            {
                throw new InvalidOperationException("Сбой сохранения");
            }

            return await saveGate.Task;
        });

        var service = new AccountsService(env, userManager, context, queueHolder, cache, Options.Create(new AccountSettings()));

        using var startGate = new ManualResetEventSlim(false);
        var confirmTasks = Enumerable.Range(0, 10)
            .Select(_ => Task.Run(async () =>
            {
                startGate.Wait();
                try
                {
                    await service.ConfirmEmailAsync("000000");
                    return string.Empty;
                }
                catch (BusinessException ex)
                {
                    return ex.Message;
                }
            }))
            .ToArray();

        startGate.Set();
        var messages = await Task.WhenAll(confirmTasks);

        Assert.Multiple(() =>
        {
            Assert.That(messages.Count(m => m.Contains("код подтверждения недействительный", StringComparison.Ordinal)), Is.EqualTo(5));
            Assert.That(messages.Count(m => m.Contains("Слишком много неудачных попыток подтверждения", StringComparison.Ordinal)), Is.EqualTo(5));
        });

        var lockedEx = Assert.ThrowsAsync<BusinessException>(() => service.ConfirmEmailAsync("654321"));
        Assert.That(lockedEx!.Message, Does.Contain("Слишком много неудачных попыток подтверждения"));

        var firstResend = service.ResendConfirmCodeAsync();
        var concurrentResendEx = Assert.ThrowsAsync<BusinessException>(() => service.ResendConfirmCodeAsync());
        Assert.That(concurrentResendEx!.Message, Does.Contain("не чаще одного раза в минуту"));

        saveGate.SetResult(1);
        await firstResend;
        Assert.That(queueHolder.MailMessages.Count, Is.EqualTo(1));

        var cooldownEx = Assert.ThrowsAsync<BusinessException>(() => service.ResendConfirmCodeAsync());
        Assert.That(cooldownEx!.Message, Does.Contain("не чаще одного раза в минуту"));

        cache.Remove($"resend-confirm-code:{user.Id}");
        saveSHouldFailSet(true);

        Assert.ThrowsAsync<InvalidOperationException>(() => service.ResendConfirmCodeAsync());
        Assert.That(cache.TryGetValue($"resend-confirm-code:{user.Id}", out _), Is.False);

        saveSHouldFailSet(false);
        await service.ResendConfirmCodeAsync();
        Assert.That(queueHolder.MailMessages.Count, Is.EqualTo(2));

        bool saveShouldFail() => saveshouldFail;
        void saveSHouldFailSet(bool value) => saveshouldFail = value;
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

    private sealed class SingleUserStore(ApplicationUser user) : IUserStore<ApplicationUser>
    {
        public void Dispose()
        {
        }

        public Task<string> GetUserIdAsync(ApplicationUser u, CancellationToken cancellationToken)
        {
            return Task.FromResult(u.Id.ToString());
        }

        public Task<string?> GetUserNameAsync(ApplicationUser u, CancellationToken cancellationToken)
        {
            return Task.FromResult(u.UserName);
        }

        public Task SetUserNameAsync(ApplicationUser u, string? userName, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task<string?> GetNormalizedUserNameAsync(ApplicationUser u, CancellationToken cancellationToken)
        {
            return Task.FromResult(u.NormalizedUserName);
        }

        public Task SetNormalizedUserNameAsync(ApplicationUser u, string? normalizedName, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task<IdentityResult> CreateAsync(ApplicationUser u, CancellationToken cancellationToken)
        {
            return Task.FromResult(IdentityResult.Success);
        }

        public Task<IdentityResult> UpdateAsync(ApplicationUser u, CancellationToken cancellationToken)
        {
            return Task.FromResult(IdentityResult.Success);
        }

        public Task<IdentityResult> DeleteAsync(ApplicationUser u, CancellationToken cancellationToken)
        {
            return Task.FromResult(IdentityResult.Success);
        }

        public Task<ApplicationUser?> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            return Task.FromResult<ApplicationUser?>(user.Id.ToString() == userId ? user : null);
        }

        public Task<ApplicationUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            return Task.FromResult<ApplicationUser?>(user);
        }
    }

    private sealed class StubDbContext(Func<CancellationToken, Task<int>> onSave)
        : ApplicationDbContext(new DbContextOptions<ApplicationDbContext>())
    {
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return onSave(cancellationToken);
        }
    }
}
