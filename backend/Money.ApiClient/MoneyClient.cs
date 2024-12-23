using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace Money.ApiClient;

public class MoneyClient
{
    public MoneyClient(HttpClient client, Action<string> log)
    {
        HttpClient = client;
        Log = log;
        Category = new CategoryClient(this);
        Operation = new OperationClient(this);
        FastOperation = new FastOperationClient(this);
        RegularOperation = new RegularOperationClient(this);
    }

    [ActivatorUtilitiesConstructor]
    public MoneyClient(HttpClient client, ILogger<MoneyClient> log) :
        this(client, p => log.LogInformation(p))
    {

    }

    public HttpClient HttpClient { get; }
    public Action<string> Log { get; }
    public ApiUser? User { get; private set; }

    public CategoryClient Category { get; }
    public OperationClient Operation { get; }
    public FastOperationClient FastOperation { get; }
    public RegularOperationClient RegularOperation { get; }

    public void SetUser(string login, string password)
    {
        User = new ApiUser
        {
            Username = login,
            Password = password,
        };
    }

    public async Task RegisterAsync(string email, string password)
    {
        JsonContent requestContent = JsonContent.Create(new { email, password });
        HttpResponseMessage response = await HttpClient.PostAsync("Account/register", requestContent);
        response.EnsureSuccessStatusCode();

        string content = await response.Content.ReadAsStringAsync();
        Log(content);
    }

    public async Task<AuthData> LoginAsync(string username, string password, CancellationToken token = default)
    {
        FormUrlEncodedContent requestContent = new([
            new KeyValuePair<string, string>("grant_type", "password"),
            new KeyValuePair<string, string>("username", username),
            new KeyValuePair<string, string>("password", password),
        ]);

        HttpResponseMessage response = await HttpClient.PostAsync("connect/token", requestContent, token);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<AuthData>(token) ?? throw new InvalidOperationException();
    }
}
