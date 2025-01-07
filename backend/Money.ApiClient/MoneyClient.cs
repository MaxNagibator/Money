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
        Debt = new(this);
        Category = new(this);
        Operation = new(this);
        FastOperation = new(this);
        RegularOperation = new(this);
    }

    [ActivatorUtilitiesConstructor]
    public MoneyClient(HttpClient client, ILogger<MoneyClient> log) :
        this(client, p => log.LogInformation("API: {Message}", p))
    {
    }

    public HttpClient HttpClient { get; }
    public Action<string> Log { get; }
    public ApiUser? User { get; private set; }

    public DebtClient Debt { get; }
    public CategoryClient Category { get; }
    public OperationClient Operation { get; }
    public FastOperationClient FastOperation { get; }
    public RegularOperationClient RegularOperation { get; }

    public void SetUser(string login, string password)
    {
        User = new()
        {
            Username = login,
            Password = password,
        };
    }

    public async Task RegisterAsync(string email, string password)
    {
        var requestContent = JsonContent.Create(new { email, password });
        var response = await HttpClient.PostAsync("Account/register", requestContent);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        Log(content);
    }

    public async Task<AuthData> LoginAsync(string username, string password, CancellationToken token = default)
    {
        var requestContent = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
        {
            new("grant_type", "password"),
            new("username", username),
            new("password", password),
        });

        var response = await HttpClient.PostAsync("connect/token", requestContent, token);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<AuthData>(token) ?? throw new InvalidOperationException();
    }
}
