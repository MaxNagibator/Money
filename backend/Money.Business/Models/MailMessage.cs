using System.Diagnostics.CodeAnalysis;

namespace Money.Business.Models;

public class MailMessage
{
    public MailMessage()
    {
    }

    [SetsRequiredMembers]
    public MailMessage(string email, string title, string body)
    {
        Email = email;
        Title = title;
        Body = body;
    }

    public required string Email { get; set; }
    public required string Title { get; set; }
    public required string Body { get; set; }
}
