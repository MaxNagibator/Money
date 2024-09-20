using System.Collections.Concurrent;
using System.Net.Http.Json;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Money.Api.Tests.TestTools;
using Money.Data;

namespace Money.Api.Tests;

[SetUpFixture]
public class Integration
{
    private static readonly ConcurrentBag<HttpClient> HttpClients = [];

    public static TestServer TestServer { get; private set; } = null!;
    //public static AuthData AuthData { get; private set; } = null!;

    public static DatabaseClient GetDatabaseClient()
    {
        IConfigurationRoot config = TestServer.Services.GetRequiredService<IConfigurationRoot>();
        string? connectionString = config.GetConnectionString("ApplicationDbContext");

        return new DatabaseClient(CreateDbContext);

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

    public static async Task Register(string email, string password)
    {
        JsonContent aasd = JsonContent.Create(new
        {
            email, password
        });

        HttpResponseMessage response = await GetHttpClient().PostAsync($"{TestServer.BaseAddress}Account/register", aasd);
        string content = await response.Content.ReadAsStringAsync();
        Console.WriteLine(content);
    }

    public static async Task<AuthData> GetAuthData(string username, string password)
    {
        //string username = "bob217@mail.ru";
        //string password = "stringA123!";
        ////string password = "222Aasdasdas123123123!";

        FormUrlEncodedContent requestContent = new([
            new KeyValuePair<string, string>("grant_type", "password"),
            new KeyValuePair<string, string>("username", username),
            new KeyValuePair<string, string>("password", password)
        ]);

        HttpResponseMessage response = await GetHttpClient().PostAsync($"{TestServer.BaseAddress}connect/token", requestContent);
        return await response.Content.ReadFromJsonAsync<AuthData>() ?? throw new InvalidOperationException();
    }

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        CustomWebApplicationFactory<Program> webHostBuilder = new();
        TestServer = webHostBuilder.Server;

        //AuthData = await GetAuthData();
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
