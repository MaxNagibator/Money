using System.Net.Http.Json;

namespace Money.ApiClient;

public class MoneyClient
{
    public MoneyClient(HttpClient client, Action<string> log)
    {
        HttpClient = client;
        Log = log;
        Category = new CategoryClient(this);
        Payment = new PaymentClient(this);
    }

    public HttpClient HttpClient { get; }
    public Action<string> Log { get; }
    public ApiUser? User { get; private set; }

    public CategoryClient Category { get; }
    public PaymentClient Payment { get; }

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

    public async Task<AuthData> LoginAsync(string username, string password)
    {
        FormUrlEncodedContent requestContent = new([
            new KeyValuePair<string, string>("grant_type", "password"),
            new KeyValuePair<string, string>("username", username),
            new KeyValuePair<string, string>("password", password),
        ]);

        HttpResponseMessage response = await HttpClient.PostAsync("connect/token", requestContent);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<AuthData>() ?? throw new InvalidOperationException();
    }
}
