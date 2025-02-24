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

    internal static bool IsEmailWithUserNameExists(TestUser user)
    {
        return Emails
            .SelectMany(x => x.Value)
            .Any(x => x.Body.Contains(user.UserName, StringComparison.InvariantCultureIgnoreCase));
    }
}
