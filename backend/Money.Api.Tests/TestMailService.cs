using Money.Business.Models;
using Money.Business.Services;
using System.Collections.Concurrent;

namespace Money.Api.Tests;

internal class TestMailService : IMailService
{
    private static readonly ConcurrentDictionary<string, List<MailMessage>> Emails = new();

    public void Send(MailMessage mailMessage)
    {
        Emails.AddOrUpdate(mailMessage.Email, [mailMessage], (_, messages) =>
        {
            messages.Add(mailMessage);
            return messages;
        });
    }

    internal static bool IsEmailWithUserNameExists(TestUser user)
    {
        return Emails
            .SelectMany(x => x.Value)
            .Any(x => x.Body.Contains(user.UserName, StringComparison.InvariantCultureIgnoreCase));
    }
}
