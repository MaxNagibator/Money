using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ServiceDiscovery;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Money.WebAssembly.CoreLib;

public static class WebAssemblyHostBuilderExtensions
{
    public static WebAssemblyHostBuilder AddServiceDefaults(this WebAssemblyHostBuilder builder)
    {
        builder.ConfigureOpenTelemetry();
        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler();
            http.AddServiceDiscovery();
        });

        builder.Services.Configure<ConfigurationServiceEndpointProviderOptions>(static options =>
        {
            options.SectionName = "Services";
            options.ShouldApplyHostNameMetadata = static _ => true;
        });

        return builder;
    }

    public static WebAssemblyHostBuilder ConfigureOpenTelemetry(this WebAssemblyHostBuilder builder)
    {
        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddHttpClientInstrumentation();
            })
            .WithTracing(tracing =>
            {
                tracing.AddHttpClientInstrumentation();
            });

        return builder;
    }
}
