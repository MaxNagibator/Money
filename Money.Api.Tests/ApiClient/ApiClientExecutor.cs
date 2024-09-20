using System.Net.Http.Json;
using Money.Api.Tests.TestTools.Entities;
using Newtonsoft.Json;

namespace Money.Api.Tests.ApiClient;

public class ApiClientExecutor(HttpClient client, Action<string> log)
{
    public ApiUser? User { get; set; }

    protected virtual string ApiPrefix => "api";

    internal void SetUser(TestUser user)
    {
        User = new ApiUser
        {
            Username = user.Login,
            Password = user.Password
        };
    }

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

        log($"method: {method}");
        log($"url: {ApiPrefix}{uri}");
        await SetAuthHeaders(requestMessage);

        if (body != null)
        {
            requestMessage.Content = JsonContent.Create(body);
            log($"body: {JsonConvert.SerializeObject(body)}");
        }

        HttpResponseMessage response = await client.SendAsync(requestMessage);
        return ProcessResponse<T>(response);
    }

    private async Task SetAuthHeaders(HttpRequestMessage requestMessage)
    {
        if (User != null)
        {
            if (User.Token == null)
            {
                AuthData authData = await Integration.GetAuthData(User.Username, User.Password);
                User.Token = authData.AccessToken;
            }

            AddHeader("Authorization", $"Bearer {User.Token}");
        }

        void AddHeader(string key, string value)
        {
            log($"{key}: {value}");
            requestMessage.Headers.Add(key, value);
        }
    }

    private ApiClientResponse<T> ProcessResponse<T>(HttpResponseMessage response)
    {
        using StreamReader responseStreamReader = new(response.Content.ReadAsStream());

        string responseContent = responseStreamReader.ReadToEnd();
        return new ApiClientResponse<T>(response.StatusCode, responseContent);
    }
}
