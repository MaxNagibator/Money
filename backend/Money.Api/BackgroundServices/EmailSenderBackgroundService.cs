using Money.Business;

namespace Money.Api.BackgroundServices;

public class EmailSenderBackgroundService(
    QueueHolder queueHolder,
    IMailService mailService,
    ILogger<EmailSenderBackgroundService> logger) : BackgroundService
{
    private readonly PeriodicTimer _timer = new(TimeSpan.FromSeconds(10));

    public override void Dispose()
    {
        base.Dispose();
        _timer.Dispose();
        GC.SuppressFinalize(this);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("{Name} останавливается", nameof(EmailSenderBackgroundService));
        _timer.Dispose();
        return base.StopAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("{Name} запускается", nameof(EmailSenderBackgroundService));

        do
        {
            try
            {
                if (queueHolder.MailMessages.TryDequeue(out var message))
                {
                    mailService.Send(message);
                }
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Ошибка при выполнении обработки отправки почты");
            }
        } while (await _timer.WaitForNextTickAsync(stoppingToken));
    }
}
