using System.Collections.Concurrent;
using System.Net.Http.Json;
using Microsoft.AspNetCore.TestHost;

namespace Money.Api.Tests;

[SetUpFixture]
public class Integration
{
    private static readonly ConcurrentBag<HttpClient> HttpClients = [];
    public static TestServer TestServer { get; private set; } = null!;
    public static AuthData AuthData { get; private set; } = null!;

    public static HttpClient GetHttpClient()
    {
        HttpClient client = TestServer.CreateClient();
        HttpClients.Add(client);
        return client;
    }

    public static async Task<AuthData> GetAuthData()
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
        return await response.Content.ReadFromJsonAsync<AuthData>() ?? throw new InvalidOperationException();
    }

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        CustomWebApplicationFactory<Program> webHostBuilder = new();
        TestServer = webHostBuilder.Server;

        AuthData = await GetAuthData();
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
