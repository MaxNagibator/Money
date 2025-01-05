using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Money.Data;
using System.Collections.Concurrent;

namespace Money.Api.Tests;

[SetUpFixture]
public class Integration
{
    private static readonly ConcurrentBag<HttpClient> HttpClients = [];

    public static TestServer TestServer { get; private set; } = null!;

    public static DatabaseClient GetDatabaseClient()
    {
        var config = TestServer.Services.GetRequiredService<IConfigurationRoot>();
        var connectionString = config.GetConnectionString(nameof(ApplicationDbContext));

        return new(CreateDbContext, new(GetHttpClient(), Console.WriteLine));

        ApplicationDbContext CreateDbContext()
        {
            DbContextOptionsBuilder<ApplicationDbContext> optionsBuilder = new();
            optionsBuilder.UseNpgsql(connectionString);
            optionsBuilder.UseSnakeCaseNamingConvention();
            var context = new ApplicationDbContext(optionsBuilder.Options);
            return context;
        }
    }

    public static HttpClient GetHttpClient()
    {
        var client = TestServer.CreateClient();
        HttpClients.Add(client);
        return client;
    }

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        CustomWebApplicationFactory<Program> webHostBuilder = new();
        webHostBuilder.Server.PreserveExecutionContext = true;

        TestServer = webHostBuilder.Server;
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        TestServer.Dispose();

        foreach (var httpClient in HttpClients.ToArray())
        {
            httpClient.Dispose();
        }
    }
}
