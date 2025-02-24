using Microsoft.Extensions.Options;
using Money.Business.Configs;
using System.Net;
using System.Net.Mail;
using MailMessage = Money.Business.Models.MailMessage;

namespace Money.Business.Services;

public interface IMailService
{
    Task SendAsync(MailMessage mailMessage, CancellationToken cancellationToken = default);
}

public class MailService(IOptions<SmtpSettings> options) : IMailService
{
    private readonly SmtpSettings _settings = options.Value;

    public async Task SendAsync(MailMessage mailMessage, CancellationToken cancellationToken = default)
    {
        using var client = Create();
        using var message = new System.Net.Mail.MailMessage(_settings.SenderEmail, mailMessage.Email, mailMessage.Title, mailMessage.Body);
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
