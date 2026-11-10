using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Money.Business.Configs;
using Money.Data.Entities;
using System.Text;
using System.Text.RegularExpressions;

namespace Money.Business.Services;

public partial class AccountsService(
    RequestEnvironment environment,
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext context,
    QueueHolder queueHolder,
    IMemoryCache memoryCache,
    IOptions<AccountSettings> options)
{
    private static readonly object ConfirmStateLock = new();
    private readonly AccountSettings _settings = options.Value;

    public async Task RegisterAsync(RegisterAccount model, CancellationToken cancellationToken = default)
    {
        if (!UserNameRegex().IsMatch(model.UserName))
        {
            throw new EntityExistsException("Извините, но имя пользователя не может содержать служебные символы.");
        }

        var user = await userManager.FindByNameAsync(model.UserName);

        if (user != null)
        {
            throw new EntityExistsException("Извините, но пользователь с таким именем уже зарегистрирован. Пожалуйста, попробуйте другое имя пользователя.");
        }

        if (model.Email != null)
        {
            user = await userManager.FindByEmailAsync(model.Email);

            if (user != null)
            {
                if (user.EmailConfirmed)
                {
                    throw new EntityExistsException("Извините, но пользователь с таким email уже зарегистрирован. Пожалуйста, попробуйте другоЙ email.");
                }

                user.Email = null;
                user.EmailConfirmCode = null;
                await userManager.UpdateAsync(user);
            }
        }

        user = new()
        {
            UserName = model.UserName,
            Email = model.Email,
        };

        if (model.Email != null)
        {
            user.EmailConfirmCode = GetCode(6);
        }

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        var result = await userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
        {
            throw new IncorrectDataException($"Ошибки: {string.Join("; ", result.Errors.Select(error => error.Description))}");
        }

        await AddNewUser(user.Id, cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        if (model.Email != null)
        {
            SendEmail(user.UserName, model.Email, user.EmailConfirmCode!);
        }
    }

    public async Task ChangePasswordAsync(string currentPassword, string newPassword)
    {
        var user = environment.AuthUser;
        if (user == null)
        {
            throw new BusinessException("Извините, но пользователь не указан.");
        }

        var domainUser = await context.DomainUsers.SingleAsync(x => x.AuthUserId == user.Id);
        if (domainUser.TransporterPassword != null)
        {
            if (!LegacyAuth.Validate(user.UserName!, currentPassword, domainUser.TransporterPassword))
            {
                throw new PermissionException("Неверное имя пользователя или пароль.");
            }

            await userManager.AddPasswordAsync(user, newPassword);
            domainUser.TransporterPassword = null;
            await context.SaveChangesAsync();
        }
        else
        {
            var result = await userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            if (!result.Succeeded)
            {
                throw new IncorrectDataException($"Ошибки: {string.Join("; ", result.Errors.Select(error => error.Description))}");
            }
        }
    }

    public async Task<int> EnsureUserIdAsync(Guid authUserId, CancellationToken cancellationToken = default)
    {
        var domainUser = await context.DomainUsers.FirstOrDefaultAsync(x => x.AuthUserId == authUserId, cancellationToken);

        if (domainUser != null)
        {
            return domainUser.Id;
        }

        return await AddNewUser(authUserId, cancellationToken);
    }

    public async Task ConfirmEmailAsync(string confirmCode, CancellationToken cancellationToken = default)
    {
        var user = environment.AuthUser;

        if (user == null)
        {
            throw new BusinessException("Извините, но пользователь не указан.");
        }

        if (user.EmailConfirmed)
        {
            throw new BusinessException("Извините, но у вас уже подтвержденный email.");
        }

        if (user.Email == null)
        {
            throw new BusinessException("Простите, но у вас не заполнен email.");
        }

        var attemptsKey = $"confirm-email-attempts:{user.Id}";

        lock (ConfirmStateLock)
        {
            var failedAttempts = memoryCache.Get<int>(attemptsKey);

            if (failedAttempts >= _settings.MaxConfirmEmailFailedAttempts)
            {
                throw new BusinessException("Слишком много неудачных попыток подтверждения. Попробуйте позже или запросите новый код.");
            }

            if (user.EmailConfirmCode != confirmCode)
            {
                memoryCache.Set(attemptsKey, failedAttempts + 1, _settings.ConfirmEmailLockoutDuration);
                throw new BusinessException("Извините, код подтверждения недействительный.");
            }
        }

        user.EmailConfirmed = true;
        await context.SaveChangesAsync(cancellationToken);

        lock (ConfirmStateLock)
        {
            memoryCache.Remove(attemptsKey);
        }
    }

    public async Task ResendConfirmCodeAsync(CancellationToken cancellationToken = default)
    {
        var user = environment.AuthUser;

        if (user == null)
        {
            throw new BusinessException("Извините, но пользователь не указан.");
        }

        if (user.EmailConfirmed)
        {
            throw new BusinessException("Извините, но у вас уже подтвержденный email.");
        }

        if (user.Email == null)
        {
            throw new BusinessException("Простите, но у вас не заполнен email.");
        }

        var cooldownKey = $"resend-confirm-code:{user.Id}";

        lock (ConfirmStateLock)
        {
            if (memoryCache.TryGetValue(cooldownKey, out _))
            {
                throw new BusinessException("Повторная отправка кода возможна не чаще одного раза в минуту.");
            }

            memoryCache.Set(cooldownKey, true, _settings.ResendConfirmCodeCooldown);
        }

        var confirmCode = GetCode(6);

        try
        {
            user.EmailConfirmCode = confirmCode;
            await context.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            lock (ConfirmStateLock)
            {
                memoryCache.Remove(cooldownKey);
            }

            throw;
        }

        lock (ConfirmStateLock)
        {
            memoryCache.Remove($"confirm-email-attempts:{user.Id}");
        }

        SendEmail(user.UserName!, user.Email, confirmCode);
    }

    private static string GetCode(int length, string allowedChars = "1234567890")
    {
        var result = new StringBuilder(length);

        while (result.Length < length)
        {
            var index = Random.Shared.Next(allowedChars.Length);
            result.Append(allowedChars[index]);
        }

        return result.ToString();
    }

    private void SendEmail(string userName, string email, string confirmCode)
    {
        const string Title = "Подтверждение регистрации";
        var body = $"Здравствуйте, {userName}!\r\nВаш код для подтверждения регистрации на сайте Филочек:\r\n{confirmCode}";

        // TODO: Стоит ли добавлять новое письмо, если уже есть в очереди письмо на тот же email
        queueHolder.MailMessages.Enqueue(new(email, Title, body));
    }

    // TODO Подумать над переносом в сервис
    private async Task<int> AddNewUser(Guid authUserId, CancellationToken cancellationToken = default)
    {
        var domainUser = new DomainUser
        {
            AuthUserId = authUserId,
        };

        await context.DomainUsers.AddAsync(domainUser, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return domainUser.Id;
    }

    [GeneratedRegex("^[a-zA-Z0-9_-]+$", RegexOptions.Compiled)]
    private static partial Regex UserNameRegex();
}
