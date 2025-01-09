using Money.Api.BackgroundServices;

namespace Money.Api.Definitions;

public class BackgroundServicesDefinition : AppDefinition
{
    public override void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.AddHostedService<RegularTaskBackgroundService>();
    }
}
