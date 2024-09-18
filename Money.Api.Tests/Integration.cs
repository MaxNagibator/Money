using System.Collections.Concurrent;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Money.Data;

namespace Money.Api.Tests;

//public class CustomWebApplicationFactory<TProgram>
//    : WebApplicationFactory<TProgram> where TProgram : class
//{
//    protected override void ConfigureWebHost(IWebHostBuilder builder)
//    {
//        base.ConfigureWebHost(builder);

//        string env = "Development";

//        IConfigurationRoot configRoot = new ConfigurationBuilder()
//            .SetBasePath(Directory.GetCurrentDirectory())
//            .AddJsonFile($"appsettings.{env}.json")
//            .Build();

//        builder.UseConfiguration(configRoot);
//        builder.UseContentRoot(Directory.GetCurrentDirectory());
//        builder.UseEnvironment(env);

//        builder.ConfigureServices(services =>
//        {
//            services.AddDbContext<ApplicationDbContext>(options =>
//            {
//                //options.UseNpgsql(builder.Configuration.GetConnectionString(nameof(ApplicationDbContext)));
//                options.UseSnakeCaseNamingConvention();
//               // options.UseOpenIddict();
//            });
//        });
//    }
//}

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
        var env = "Development";
        var configRoot = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"appsettings.{env}.json")
            .Build();
        var webHostBuilder = new WebHostBuilder()
            .UseEnvironment(env)
            .UseContentRoot(Directory.GetCurrentDirectory())
            .UseConfiguration(configRoot)
            .ConfigureServices(services =>
            {
                services.AddSingleton(configRoot);
            })
       //.ConfigureLogging((hostingContext, logging) =>
       //{
       //    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
       //    logging.AddConsole();
       //    logging.AddDebug();
       //})
       .UseStartup<Startup>();

        TestServer = new TestServer(webHostBuilder);
        //CustomWebApplicationFactory<Program> webHostBuilder = new CustomWebApplicationFactory<Program>();
        //TestServer = webHostBuilder.Server;
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
