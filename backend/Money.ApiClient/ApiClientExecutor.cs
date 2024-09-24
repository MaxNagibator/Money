using System.Net.Http.Json;
using System.Text.Json;

namespace Money.ApiClient;

public class ApiClientExecutor(MoneyClient apiClient)
{
    protected virtual string ApiPrefix => "api";

    protected async Task<ApiClientResponse<T>> GetAsync<T>(string uri)
    {
        return await SendWithBody<T>(HttpMethod.Get, uri);
    }

    protected async Task<ApiClientResponse<T>> PostAsync<T>(string uri, object body)
    {
        return await SendWithBody<T>(HttpMethod.Post, uri, body);
    }

    protected async Task<ApiClientResponse<T>> PatchAsync<T>(string uri, object body)
    {
        return await SendWithBody<T>(HttpMethod.Patch, uri, body);
    }

    protected async Task<ApiClientResponse> DeleteAsync(string uri)
    {
        return await SendWithBody<string>(HttpMethod.Delete, uri);
    }

    private async Task<ApiClientResponse<T>> SendWithBody<T>(HttpMethod method, string uri, object? body = null)
    {
        using HttpRequestMessage requestMessage = new(method, $"{ApiPrefix}{uri}");

        apiClient.Log($"method: {method}");
        apiClient.Log($"url: {ApiPrefix}{uri}");
        await SetAuthHeaders(requestMessage);

        if (body != null)
        {
            requestMessage.Content = JsonContent.Create(body);
            apiClient.Log($"body: {JsonSerializer.Serialize(body)}");
        }

        HttpResponseMessage response = await apiClient.HttpClient.SendAsync(requestMessage);
        return ProcessResponse<T>(response);
    }

    private async Task SetAuthHeaders(HttpRequestMessage requestMessage)
    {
        ApiUser? user = apiClient.User;

        if (user == null)
        {
            return;
        }

        if (user.Token == null)
        {
            AuthData authData = await apiClient.LoginAsync(user.Username, user.Password);
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

    private ApiClientResponse<T> ProcessResponse<T>(HttpResponseMessage response)
    {
        using StreamReader responseStreamReader = new(response.Content.ReadAsStream());

        string responseContent = responseStreamReader.ReadToEnd();
        apiClient.Log("response: " + responseContent);
        return new ApiClientResponse<T>(response.StatusCode, responseContent);
    }
}
