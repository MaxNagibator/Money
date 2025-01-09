namespace Domiki.Web.Business;

public class RegularTaskBackgroundService : BackgroundService
{
    private IServiceProvider _serviceProvider;
    private DateTime _regularTaskRunLastExecuteDate;

    public RegularTaskBackgroundService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _regularTaskRunLastExecuteDate = DateTime.Now.Date.AddDays(-1);
    }

    protected override async Task ExecuteAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            if (_regularTaskRunLastExecuteDate < DateTime.Now.Date)
            {
                try
                {
                    using (IServiceScope scope = _serviceProvider.CreateScope())
                    {
                        var service = scope.ServiceProvider.GetRequiredService<RegularOperationService>();
                        await service.RunRegularTaskAsync(token);
                    }

                    _regularTaskRunLastExecuteDate = DateTime.Now.Date;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    await Task.Delay(10000);
                }
            }
        }
    }
}
