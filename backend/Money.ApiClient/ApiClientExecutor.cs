using System.Net.Http.Json;
using System.Text.Json;

namespace Money.ApiClient;

public class ApiClientExecutor(MoneyClient apiClient)
{
    protected virtual string ApiPrefix => "api";

    protected async Task<ApiClientResponse<T>> GetAsync<T>(string uri, CancellationToken token = default)
    {
        return await SendWithBody<T>(HttpMethod.Get, uri, token: token);
    }

    protected async Task<ApiClientResponse<T>> PostAsync<T>(string uri, object? body = null, CancellationToken token = default)
    {
        return await SendWithBody<T>(HttpMethod.Post, uri, body, token);
    }

    protected async Task<ApiClientResponse> PostAsync(string uri, object? body = null, CancellationToken token = default)
    {
        return await SendWithBody<string>(HttpMethod.Post, uri, body, token);
    }

    protected async Task<ApiClientResponse> PutAsync(string uri, object? body = null, CancellationToken token = default)
    {
        return await SendWithBody<string>(HttpMethod.Put, uri, body, token);
    }

    protected async Task<ApiClientResponse<T>> PatchAsync<T>(string uri, object? body = null, CancellationToken token = default)
    {
        return await SendWithBody<T>(HttpMethod.Patch, uri, body, token);
    }

    protected async Task<ApiClientResponse> DeleteAsync(string uri, CancellationToken token = default)
    {
        return await SendWithBody<string>(HttpMethod.Delete, uri, token: token);
    }

    private async Task<ApiClientResponse<T>> SendWithBody<T>(HttpMethod method, string uri, object? body = null, CancellationToken token = default)
    {
        using var requestMessage = new HttpRequestMessage(method, $"{ApiPrefix}{uri}");

        apiClient.Log($"method: {method}");
        apiClient.Log($"url: {ApiPrefix}{uri}");
        await SetAuthHeaders(requestMessage, token);

        if (body != null)
        {
            requestMessage.Content = JsonContent.Create(body);
            apiClient.Log($"body: {JsonSerializer.Serialize(body)}");
        }

        var response = await apiClient.HttpClient.SendAsync(requestMessage, token);
        return ProcessResponse<T>(response, token);
    }

    private async Task SetAuthHeaders(HttpRequestMessage requestMessage, CancellationToken token = default)
    {
        var user = apiClient.User;

        if (user == null)
        {
            return;
        }

        if (user.Token == null)
        {
            var authData = await apiClient.LoginAsync(user.Username, user.Password, token);
            user.AuthData = authData;
        }

        AddHeader("Authorization", $"Bearer {user.Token}");

        return;

        void AddHeader(string key, string value)
        {
            apiClient.Log($"{key}: {value}");
            requestMessage.Headers.Add(key, value);
        }
    }

    private ApiClientResponse<T> ProcessResponse<T>(HttpResponseMessage response, CancellationToken token = default)
    {
        using var responseStreamReader = new StreamReader(response.Content.ReadAsStream(token));

        var responseContent = responseStreamReader.ReadToEnd();
        apiClient.Log("response: " + responseContent);
        return new(response.StatusCode, responseContent);
    }
}
