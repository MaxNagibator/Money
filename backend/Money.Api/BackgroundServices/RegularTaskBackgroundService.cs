namespace Money.Api.BackgroundServices;

public class RegularTaskBackgroundService(IServiceProvider serviceProvider, ILogger<RegularTaskBackgroundService> logger) : BackgroundService
{
    private readonly PeriodicTimer _timer = new(TimeSpan.FromSeconds(10));
    private DateTime _lastExecuteDate = DateTime.Now.Date.AddDays(-1);

    public override Task StopAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("{Name} останавливается", nameof(RegularTaskBackgroundService));
        _timer.Dispose();
        return base.StopAsync(stoppingToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("{Name} запускается", nameof(RegularTaskBackgroundService));

        do
        {
            logger.LogDebug("Проверка необходимости выполнения задачи: "
                            + "Последняя дата выполнения - {LastExecuteDate}, "
                            + "Текущая дата - {CurrentDate}", _lastExecuteDate, DateTime.Now.Date);

            if (_lastExecuteDate >= DateTime.Now.Date)
            {
                logger.LogDebug("Задача пропущена, так как уже была выполнена сегодня");
                continue;
            }

            try
            {
                await using (var scope = serviceProvider.CreateAsyncScope())
                {
                    var service = scope.ServiceProvider.GetRequiredService<RegularOperationService>();
                    await service.RunRegularTaskAsync(stoppingToken);
                }

                _lastExecuteDate = DateTime.Now.Date;
                logger.LogDebug("Задача выполнена успешно. Обновлена последняя дата выполнения до {LastExecuteDate}", _lastExecuteDate);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Ошибка при выполнении регулярной задачи");
            }
        } while (await _timer.WaitForNextTickAsync(stoppingToken));
    }
}
