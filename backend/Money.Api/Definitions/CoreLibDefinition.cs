using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Money.Api.Definitions;

public class CoreLibDefinition : AppDefinition
{
    public override int ApplicationOrderIndex => -1;

    public override void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler();
            http.AddServiceDiscovery();
        });
    }

    public override void ConfigureApplication(WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            return;
        }

        app.MapHealthChecks("/health");

        app.MapHealthChecks("/alive", new()
        {
            Predicate = r => r.Tags.Contains("live"),
        });
    }
}
