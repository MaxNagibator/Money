namespace Money.Business.Services;

public class QueueHolder
{
    public Queue<MailMessage> MailMessages { get; set; } = new Queue<MailMessage>();
}

public class MailMessage(string Email, string Title, string Body)
{

}
