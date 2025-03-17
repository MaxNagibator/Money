using Microsoft.Extensions.Options;
using Money.Business;
using Money.Common;
using System.Threading.Channels;
using MailMessage = Money.Business.Models.MailMessage;

namespace Money.Api.BackgroundServices;

public class EmailSenderBackgroundService(
    QueueHolder queueHolder,
    IMailsService mailsService,
    ILogger<EmailSenderBackgroundService> logger,
    IOptions<EmailSenderSettings> options) : BackgroundService
{
    private readonly EmailSenderSettings _settings = options.Value;
    private readonly Channel<MailMessage> _retryChannel = Channel.CreateUnbounded<MailMessage>();
    private readonly SemaphoreSlim _semaphore = new(Environment.ProcessorCount * 2);
    private PeriodicTimer _timer = null!;

    public override void Dispose()
    {
        _timer.Dispose();
        _semaphore.Dispose();
        base.Dispose();
        GC.SuppressFinalize(this);
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new(_settings.ProcessingInterval);

        logger.LogInformation("{ServiceName} запущен с интервалом {Interval}", nameof(EmailSenderBackgroundService), _settings.ProcessingInterval);
        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("{Name} останавливается", nameof(EmailSenderBackgroundService));
        return base.StopAsync(cancellationToken);
    }

    public async Task ForceExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Принудительный запуск обработки очереди");

            using (var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            {
                linkedSource.CancelAfter(_settings.ProcessingInterval);
                await ProcessMessagesAsync(linkedSource.Token);
            }

            logger.LogInformation("Принудительная обработка завершена");
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Принудительная обработка прервана");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при принудительной обработке");
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var processingTask = ProcessMessagesAsync(stoppingToken);
        var retryTask = ProcessRetriesAsync(stoppingToken);

        do
        {
            await processingTask;
            processingTask = ProcessMessagesAsync(stoppingToken);
        } while (await _timer.WaitForNextTickAsync(stoppingToken));

        await Task.WhenAll(processingTask, retryTask);
    }

    private async Task ProcessMessagesAsync(CancellationToken cancellationToken)
    {
        var batchSize = 0;
        var tasks = new List<Task>();

        while (queueHolder.MailMessages.TryDequeue(out var message))
        {
            tasks.Add(ProcessSingleMessageAsync(message, cancellationToken));

            if (++batchSize >= _settings.MaxBatchSize)
            {
                break;
            }
        }

        await Task.WhenAll(tasks);
        logger.LogDebug("Обработано {BatchSize} сообщений", batchSize);
    }

    private async Task ProcessSingleMessageAsync(MailMessage message, CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            var stopwatch = ValueStopwatch.StartNew();
            await mailsService.SendAsync(message, cancellationToken);

            logger.LogInformation("Сообщение {MessageId} отправлено за {Elapsed} мс", message.Id, stopwatch.GetElapsedTime().TotalMilliseconds);
        }
        catch (OperationCanceledException)
        {
            queueHolder.MailMessages.Enqueue(message);
            logger.LogWarning("Операция отменена, сообщение {MessageId} возвращено в очередь", message.Id);
        }
        catch (Exception exception)
        {
            logger.LogCritical(exception, "Критическая ошибка при обработке сообщения {MessageId}", message.Id);
            await HandleRetryAsync(message, exception);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task HandleRetryAsync(MailMessage message, Exception exception)
    {
        if (message.RetryCount >= _settings.MaxRetries)
        {
            logger.LogError(exception, "Достигнут максимум повторов для сообщения {MessageId}", message.Id);
            return;
        }

        message.RetryCount++;
        var delay = CalculateRetryDelay(message.RetryCount);

        logger.LogWarning(exception, "Повторная попытка {RetryCount} для сообщения {MessageId} через {Delay} с",
            message.RetryCount, message.Id, delay.TotalSeconds);

        await _retryChannel.Writer.WriteAsync(message);
    }

    private async Task ProcessRetriesAsync(CancellationToken cancellationToken)
    {
        await foreach (var message in _retryChannel.Reader.ReadAllAsync(cancellationToken))
        {
            var delay = CalculateRetryDelay(message.RetryCount);
            await Task.Delay(delay, cancellationToken);
            queueHolder.MailMessages.Enqueue(message);
        }
    }

    private TimeSpan CalculateRetryDelay(int retryCount)
    {
        return TimeSpan.FromSeconds(_settings.RetryBaseDelaySeconds * Math.Pow(2, retryCount));
    }
}
