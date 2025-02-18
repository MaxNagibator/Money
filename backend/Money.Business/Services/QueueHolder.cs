using System.Collections.Concurrent;

namespace Money.Business.Services;

public class QueueHolder
{
    public ConcurrentQueue<MailMessage> MailMessages { get; set; } = new ConcurrentQueue<MailMessage>();
}

public class MailMessage
{
    public MailMessage()
    {

    }

    public MailMessage(string email, string title, string body)
    {
        Email = email;
        Title = title;
        Body = body;
    }

    public string Email { get; set; }
    public string Title { get; set; }
    public string Body { get; set; }
}
