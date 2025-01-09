namespace Money.Api.BackgroundServices;

public class RegularTaskBackgroundService(IServiceProvider serviceProvider, ILogger<RegularTaskBackgroundService> logger) : BackgroundService
{
    private DateTime _lastExecuteDate = DateTime.Now.Date.AddDays(-1);

    protected override async Task ExecuteAsync(CancellationToken token)
    {
        while (token.IsCancellationRequested == false)
        {
            if (_lastExecuteDate >= DateTime.Now.Date)
            {
                continue;
            }

            try
            {
                await using (var scope = serviceProvider.CreateAsyncScope())
                {
                    var service = scope.ServiceProvider.GetRequiredService<RegularOperationService>();
                    await service.RunRegularTaskAsync(token);
                }

                _lastExecuteDate = DateTime.Now.Date;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Ошибка при выполнении регулярной задачи");
            }

            await Task.Delay(10000, token);
        }
    }
}
