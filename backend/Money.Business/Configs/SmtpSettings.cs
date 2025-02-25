using System.Security;

namespace Money.Business.Configs;

public class SmtpSettings
{
    public required string Host { get; init; }
    public required int Port { get; init; }
    public required string UserName { get; init; }
    public required string Password { get; init; }
    public required bool EnableSSL { get; init; }
    public required string SenderEmail { get; init; }
}
