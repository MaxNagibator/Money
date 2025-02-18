using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Money.Business.Services;

namespace Money.Api.Tests;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var env = "Development";

        var configRoot = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile($"appsettings.{env}.json")
            .Build();

        builder.ConfigureServices(services =>
        {
            services.AddSingleton(configRoot);
            services.AddSingleton<IMailService, TestMailService>();
        });

        builder.UseConfiguration(configRoot);
        builder.UseContentRoot(Directory.GetCurrentDirectory());
        builder.UseEnvironment(env);

    }
}
