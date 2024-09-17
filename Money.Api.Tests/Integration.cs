using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Money.Api.Middlewares;
using Money.Business.Services;
using Money.Business;
using Money.Common;
using System.Collections.Concurrent;
using System.Net.Http;

namespace Money.Api.Tests;

[SetUpFixture]
public class Integration
{
    //public static Configuration Settings { get; private set; }
    public static TestServer TestServer { get; private set; }
    private static readonly ConcurrentBag<HttpClient> _httpClients = new();

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
       //.ConfigureServices(services =>
       //{
       //    services.AddSingleton(configRoot);

       //    services.AddScoped<RequestEnvironment>();
       //    services.AddScoped<AccountService>();
       //    services.AddScoped<AuthService>();
       //    services.AddScoped<PaymentCategoryService>();
       //})
      //.ConfigureTestServices(services =>
      //{
      //    services.AddScoped<IFieldApiService, FieldApiServiceMock>();
      //})

      .UseStartup<Startup>()
       ;

        TestServer = new TestServer(webHostBuilder);
        //var config = TestServer.Services.GetRequiredService<IConfigurationRoot>();
        //var connectionString = config.GetConnectionString("PreciseAgricultureDb");

        //var appSettings = new Configuration();
        //appSettings.PreciseAgricultureDbConnectionString = connectionString ?? throw new Exception("PreciseAgricultureDbConnectionString not found");
        //Settings = appSettings;
    }

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new EnumerationConverterFactory());
                });

            services.AddLocalization();
            services.AddHttpContextAccessor();
            services.AddResponseCaching();
            services.AddMemoryCache();

            services.AddScoped<RequestEnvironment>();
            services.AddScoped<AccountService>();
            services.AddScoped<AuthService>();
            services.AddScoped<PaymentCategoryService>();
        }

        // Use this method to configure the HTTP request pipeline.  
        public void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env)
        {
            app.UseMiddleware<ExceptionHandlingMiddleware>();
            app.UseMiddleware<AuthMiddleware>();
            //app.UseHttpsRedirection();

            //app.UseEndpoints(endpoints =>
            //{
            //    endpoints.MapControllers();
            //});
        }
    }

    public static HttpClient GetHttpClient()
    {
        var client = TestServer.CreateClient();
        _httpClients.Add(client);
        return client;
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        TestServer.Dispose();
        foreach (var httpClient in _httpClients.ToArray())
            httpClient.Dispose();
    }
}