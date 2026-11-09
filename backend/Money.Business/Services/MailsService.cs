using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Money.Business.Configs;
using System.Net;
using System.Net.Mail;
using MailMessage = Money.Business.Models.MailMessage;

namespace Money.Business.Services;

public interface IMailsService
{
    Task SendAsync(MailMessage mailMessage, CancellationToken cancellationToken = default);
}

public class MailsService(
    IOptions<SmtpSettings> options,
    ILogger<MailsService> logger) : IMailsService
{
    private readonly SmtpSettings _settings = options.Value;

    public async Task SendAsync(MailMessage mailMessage, CancellationToken cancellationToken = default)
    {
        if (_settings.SendToConsole)
        {
            logger.LogInformation(
                """
                Email intercepted (SMTP sending is disabled)
                To: {ReceiverEmail}
                Subject: {Title}

                {Body}
                """,
                mailMessage.ReceiverEmail,
                mailMessage.Title,
                mailMessage.Body);
            return;
        }

        using var client = Create();
        using var message = new System.Net.Mail.MailMessage(_settings.SenderEmail, mailMessage.ReceiverEmail, mailMessage.Title, mailMessage.Body);
        await client.SendMailAsync(message, cancellationToken);
    }

    private SmtpClient Create()
    {
        return new(_settings.Host, _settings.Port)
        {
            EnableSsl = _settings.EnableSSL,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(_settings.UserName, _settings.Password),
        };
    }
}
