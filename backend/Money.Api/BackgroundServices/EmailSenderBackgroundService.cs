using Money.Business;

namespace Money.Api.BackgroundServices;

public class EmailSenderBackgroundService(
    QueueHolder queueHolder,
    IMailsService mailsService,
    ILogger<EmailSenderBackgroundService> logger) : BackgroundService
{
    public static TimeSpan Delay { get; set; } = TimeSpan.FromSeconds(10);
    private PeriodicTimer _timer = null!;

    public override void Dispose()
    {
        base.Dispose();
        _timer.Dispose();
        GC.SuppressFinalize(this);
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new(Delay);
        return base.StartAsync(cancellationToken);
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
            if (queueHolder.MailMessages.TryDequeue(out var message) == false)
            {
                continue;
            }

            logger.LogInformation("Обработка сообщения");

            try
            {
                await mailsService.SendAsync(message, stoppingToken);
                logger.LogInformation("Сообщение успешно отправлено");
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Ошибка при отправке сообщения");

                queueHolder.MailMessages.Enqueue(message);
                logger.LogWarning("Сообщение возвращено в очередь для повторной отправки");
            }
        } while (await _timer.WaitForNextTickAsync(stoppingToken));
    }
}
