using System.Net;
using System.Net.Http.Json;
using Money.Business.Enums;
using Newtonsoft.Json;

namespace Money.Api.Tests;

internal class CategoryClient : ApiClientExecutor
{
    protected override string ApiPrefix => "";

    public CategoryClient(HttpClient client, Action<string> log) : base(client, log)
    {
    }

    public async Task<ApiClientResponse<GetCategoriesModel>> Get(int? type = null)
    {
        var paramUri = type == null ? "" : "?type=" + type;
        return await GetAsync<GetCategoriesModel>($"/Categories" + paramUri);
    }

    public class GetCategoriesModel
    {
        public CategoryValue[] Categories { get; set; }

        public class CategoryValue
        {
            public int Id { get; set; }

            public required string Name { get; set; }

            public int? ParentId { get; set; }

            public int? Order { get; set; }

            public string? Color { get; set; }

            public PaymentTypes PaymentType { get; set; }
        }
    }
}

public class ApiUser
{
}

public class ApiClientExecutor
{
    protected virtual string ApiPrefix => "api";

    protected HttpClient _client;

    public ApiClientExecutor(HttpClient client, Action<string> log)
    {
        _client = client;
        _log = log;
    }

    private Action<string> _log;
    public ApiUser? User { get; set; }

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
        using (var requestMessage = new HttpRequestMessage(method, ApiPrefix + uri))
        {
            Console.WriteLine("method: " + method);
            Console.WriteLine("url: " + ApiPrefix + uri);
            SetAuthHeaders(requestMessage);
            if (body != null)
            {
                requestMessage.Content = JsonContent.Create(body);
                Console.WriteLine("body: " + JsonConvert.SerializeObject(body));
            }
            var response = await _client.SendAsync(requestMessage);
            return ProcessResponse<T>(response);
        }
    }

    private void SetAuthHeaders(HttpRequestMessage requestMessage)
    {
        if (Integration.AuthData != null)
        {
            AddHeader("Authorization", "Bearer " + Integration.AuthData.AccessToken);
        }

        //if (User != null)
        //{
        //    var userJson = JsonConvert.SerializeObject(User.Data);
        //    var valueBytes = Encoding.UTF8.GetBytes(userJson);
        //    var userHeader = Convert.ToBase64String(valueBytes);
        //    var modulesHeader = ((int)User.Modules).ToString();
        //    var permissionHeader = User.Permissions;
        //    AddHeader(Headers.User, userHeader);
        //    AddHeader(Headers.Modules, modulesHeader);
        //    AddHeader(Headers.Permissions, permissionHeader);
        //}

        void AddHeader(string key, string value)
        {
            Console.WriteLine(key + ": " + value);
            requestMessage.Headers.Add(key, value);
        }
    }

    private ApiClientResponse<T> ProcessResponse<T>(HttpResponseMessage response)
    {
        using (var responseStreamReader = new StreamReader(response.Content.ReadAsStream()))
        {
            var responseContent = responseStreamReader.ReadToEnd();
            return new ApiClientResponse<T>(response.StatusCode, responseContent);
        }
    }
}

public class ApiClientResponse
{
    public ApiClientResponse(HttpStatusCode code, string content)
    {
        Code = code;
        StringContent = content;
    }

    /// <summary>
    /// HTTP код ответа.
    /// </summary>
    public HttpStatusCode Code { get; set; }

    public string StringContent { get; set; }
}

public class ApiClientResponse<T> : ApiClientResponse
{
    public ApiClientResponse(HttpStatusCode code, string content) : base(code, content)
    {
    }

    public T Content
    {
        get
        {
            if (typeof(T) == typeof(string))
            {
                return (T)Convert.ChangeType(StringContent, typeof(T));
            }

            return JsonConvert.DeserializeObject<T>(StringContent);
        }
    }
}


public static class ApiClientExtentions
{
    public static ApiClientResponse<T> IsSuccess<T>(this ApiClientResponse<T> response)
    {
        Assert.That(response.Code, Is.EqualTo(System.Net.HttpStatusCode.OK), response.StringContent);
        return response;
    }

    public static ApiClientResponse IsSuccess(this ApiClientResponse response)
    {
        Assert.That(response.Code, Is.EqualTo(System.Net.HttpStatusCode.OK));
        return response;
    }
}
