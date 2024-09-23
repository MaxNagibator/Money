using System.Net.Http.Json;

namespace Money.ApiClient;

public class MoneyClient
{
    public ApiUser? User { get; set; }
    public Action<string> Log;
    public HttpClient HttpClient;

    public MoneyClient(HttpClient client, Action<string> log)
    {
        HttpClient = client;
        Log = log;
        Category = new CategoryClient(this);
    }

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
        JsonContent user = JsonContent.Create(new { email, password });

        HttpResponseMessage response = await HttpClient.PostAsync("Account/register", user);
        string content = await response.Content.ReadAsStringAsync();

        Console.WriteLine(content);
    }

    public async Task<AuthData> LoginAsync(string username, string password)
    {
        FormUrlEncodedContent requestContent = new([
            new KeyValuePair<string, string>("grant_type", "password"),
            new KeyValuePair<string, string>("username", username),
            new KeyValuePair<string, string>("password", password)
        ]);

        HttpResponseMessage response = await HttpClient.PostAsync("connect/token", requestContent);
        return await response.Content.ReadFromJsonAsync<AuthData>() ?? throw new InvalidOperationException();
    }

    public CategoryClient Category { get; }
}
