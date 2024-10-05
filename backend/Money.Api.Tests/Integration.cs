using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Money.Api.Tests.TestTools;
using Money.ApiClient;
using Money.Data;
using System.Collections.Concurrent;
using System.Globalization;

namespace Money.Api.Tests;

[SetUpFixture]
public class Integration
{
    private static readonly ConcurrentBag<HttpClient> HttpClients = [];

    public static TestServer TestServer { get; private set; } = null!;

    public static DatabaseClient GetDatabaseClient()
    {
        IConfigurationRoot config = TestServer.Services.GetRequiredService<IConfigurationRoot>();
        string? connectionString = config.GetConnectionString(nameof(ApplicationDbContext));

        return new DatabaseClient(CreateDbContext, new MoneyClient(GetHttpClient(), Console.WriteLine));

        ApplicationDbContext CreateDbContext()
        {
            DbContextOptionsBuilder<ApplicationDbContext> optionsBuilder = new();
            optionsBuilder.UseNpgsql(connectionString);
            optionsBuilder.UseSnakeCaseNamingConvention();
            ApplicationDbContext context = new(optionsBuilder.Options);
            return context;
        }
    }

    public static HttpClient GetHttpClient()
    {
        HttpClient client = TestServer.CreateClient();
        HttpClients.Add(client);
        return client;
    }

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        CustomWebApplicationFactory<Program> webHostBuilder = new();
        TestServer = webHostBuilder.Server;
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        TestServer.Dispose();

        foreach (HttpClient httpClient in HttpClients.ToArray())
        {
            httpClient.Dispose();
        }
    }
}
