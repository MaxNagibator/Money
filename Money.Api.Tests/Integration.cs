using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Money.Api.Tests;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        string env = "Development";

        IConfigurationRoot configRoot = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile($"appsettings.{env}.json")
            .Build();

       builder.ConfigureServices(services =>
       {
           services.AddSingleton(configRoot);
       });

        builder.UseConfiguration(configRoot);
        builder.UseContentRoot(Directory.GetCurrentDirectory());
        builder.UseEnvironment(env);
    }
}

[SetUpFixture]
public class Integration
{
    private static readonly ConcurrentBag<HttpClient> _httpClients = new();
    public static TestServer TestServer { get; private set; }
    public static AuthData? AuthData { get; private set; }

    public static HttpClient GetHttpClient()
    {
        HttpClient client = TestServer.CreateClient();
        _httpClients.Add(client);
        return client;
    }

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        CustomWebApplicationFactory<Program> webHostBuilder = new CustomWebApplicationFactory<Program>();
        TestServer = webHostBuilder.Server;

       AuthData = await GetAuthData();
    }

    public static async Task<AuthData?> GetAuthData()
    {
        string username = "bob217@mail.ru";
        string password = "stringA123!";
        //string password = "222Aasdasdas123123123!";

        FormUrlEncodedContent requestContent = new([
            new KeyValuePair<string, string>("grant_type", "password"),
            new KeyValuePair<string, string>("username", username),
            new KeyValuePair<string, string>("password", password)
        ]);

        HttpResponseMessage response = await GetHttpClient().PostAsync($"{TestServer.BaseAddress}connect/token", requestContent);
        return await response.Content.ReadFromJsonAsync<AuthData>();
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

public record AuthData(
    [property: JsonPropertyName("access_token")]
    string AccessToken,
    [property: JsonPropertyName("token_type")]
    string TokenType,
    [property: JsonPropertyName("expires_in")]
    int ExpiresIn);
