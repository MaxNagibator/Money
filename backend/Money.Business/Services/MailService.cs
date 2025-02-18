namespace Money.Business.Services;

public interface IMailService
{
    void Send(MailMessage mailMessage);
}

public class MailService() : IMailService
{
    public void Send(MailMessage mailMessage)
    {
        throw new NotImplementedException();
    }
}
