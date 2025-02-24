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

    internal static bool IsEmailWithUserNameExists(string userName)
    {
        return GetEmailWithUserNameExists(userName) != null;
    }

    internal static MailMessage GetEmailWithUserNameExists(string userName)
    {
        return Emails
            .SelectMany(x => x.Value)
            .FirstOrDefault(x => x.Body.Contains(userName, StringComparison.InvariantCultureIgnoreCase));
    }
}
