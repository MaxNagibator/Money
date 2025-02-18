using Money.Business.Services;
using System.Collections.Concurrent;

namespace Money.Api.Tests;

internal class TestMailService : IMailService
{
    public static ConcurrentDictionary<string, List<MailMessage>> Emails = new ConcurrentDictionary<string, List<MailMessage>>();

    public void Send(MailMessage mailMessage)
    {
        Emails.AddOrUpdate(mailMessage.Email, new List<MailMessage> { mailMessage }, (x, y) => { y.Add(mailMessage); return y; });
    }
}
