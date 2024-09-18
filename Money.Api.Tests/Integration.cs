using System.Collections.Concurrent;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;

namespace Money.Api.Tests;

public class CustomWebApplicationFactory<TProgram>
    : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        string env = "Development";

        IConfigurationRoot configRoot = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile($"appsettings.{env}.json")
            .Build();

        builder.UseConfiguration(configRoot);
        builder.UseContentRoot(Directory.GetCurrentDirectory());
        builder.UseEnvironment("Development");
    }
}

[SetUpFixture]
public class Integration
{
    private static readonly ConcurrentBag<HttpClient> _httpClients = new();
    public static TestServer TestServer { get; private set; }

    public static HttpClient GetHttpClient()
    {
        HttpClient client = TestServer.CreateClient();
        _httpClients.Add(client);
        return client;
    }

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        CustomWebApplicationFactory<Program> webHostBuilder = new CustomWebApplicationFactory<Program>();
        TestServer = webHostBuilder.Server;
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        TestServer.Dispose();

        foreach (HttpClient httpClient in _httpClients.ToArray())
        {
            httpClient.Dispose();
        }
    }
}
