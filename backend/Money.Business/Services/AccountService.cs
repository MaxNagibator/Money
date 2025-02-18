using Microsoft.AspNetCore.Identity;
using Money.Data.Entities;
using System.Text;

namespace Money.Business.Services;

public class AccountService(UserManager<ApplicationUser> userManager, ApplicationDbContext context, QueueHolder queueHolder)
{
    private static Random _random = new Random();
    private string GetCode(int length)
    {
        var str = new StringBuilder();
        for (var i = 0; i < length; i++)
        {
            var int1 = _random.Next();
            str.Append(int1);
        }
        return str.ToString();
    }

    public async Task RegisterAsync(RegisterModel model, CancellationToken cancellationToken = default)
    {
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
                    throw new EntityExistsException("Извините, но пользователь с таким email уже зарегистрирован. Пожалуйста, попробуйте другое имя пользователя.");
                }
                else
                {
                    user.Email = null;
                    user.EmailConfirmCode = null;
                    await userManager.UpdateAsync(user);
                }
            }
        }
        user = new()
        {
            UserName = model.UserName,
            Email = model.Email,
        };

        if (model.Email == null)
        {
            user.EmailConfirmCode = GetCode(6);
        }
        var result = await userManager.CreateAsync(user, model.Password);
        if (result.Succeeded == false)
        {
            throw new IncorrectDataException($"Ошибки: {string.Join("; ", result.Errors.Select(error => error.Description))}");
        }

        await AddNewUser(user.Id, cancellationToken);

        if (model.Email != null)
        {
            var title = "Подтверждение регистрации";
            var body = $"Здравствуйте, {user.UserName}\r\nВаш код для подтверждения регистрации на сайте филочек " + user.EmailConfirmCode;
            queueHolder.MailMessages.Enqueue(new MailMessage(model.Email, title, body));
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

    // TODO Подумать над переносом в сервис
    private async Task<int> AddNewUser(Guid authUserId, CancellationToken cancellationToken)
    {
        var domainUser = new DomainUser
        {
            AuthUserId = authUserId,
        };

        await context.DomainUsers.AddAsync(domainUser, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return domainUser.Id;
    }
}
