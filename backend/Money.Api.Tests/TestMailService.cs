using Money.Business.Models;
using Money.Business.Services;
using System.Collections.Concurrent;

namespace Money.Api.Tests;

internal class TestMailService : IMailService
{
    private static readonly ConcurrentDictionary<string, List<MailMessage>> Emails = new();

    public Task SendAsync(MailMessage mailMessage, CancellationToken cancellationToken = default)
    {
        Emails.AddOrUpdate(mailMessage.Email, [mailMessage], (_, messages) =>
        {
            messages.Add(mailMessage);
            return messages;
        });

        return Task.CompletedTask;
    }

    internal static IEnumerable<MailMessage> GetEmailsByUserName(string userName)
    {
        return Emails
            .SelectMany(x => x.Value)
            .Where(x => x.Body.Contains(userName, StringComparison.InvariantCultureIgnoreCase));
    }

    internal static MailMessage? GetEmailByUserName(string userName)
    {
        return GetEmailsByUserName(userName).FirstOrDefault();
    }

    internal static bool IsEmailWithUserNameExists(string userName)
    {
        return GetEmailByUserName(userName) != null;
    }

    internal static string GetConfirmCode(MailMessage email)
    {
        return email.Body.Split("\r\n")[^1];
    }

    internal static string? GetConfirmCode(string userName)
    {
        var email = GetEmailByUserName(userName);
        return email == null ? null : GetConfirmCode(email);
    }
}
