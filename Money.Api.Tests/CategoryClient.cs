using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Money.Business.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http.Json;

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
        string token;
        using (var requestMessage = new HttpRequestMessage(method, "/connect/token"))
        {
            Console.WriteLine("method: " + method);
            Console.WriteLine("url: " + ApiPrefix + uri);
            SetAuthHeaders(requestMessage);

            var data = new AuthData2
            {
                grant_type = "password",
                username = "bob217@mail.ru",
                password = "stringA123!",
            };

            var content = new MultipartFormDataContent();
            content.Add(new StringContent("grant_type"), "password");
            content.Add(new StringContent("username"), "bob217@mail.ru");
            content.Add(new StringContent("password"), "stringA123!");
        

                    requestMessage.Content = content;// JsonContent.Create(data);
                //requestMessage.Content = JsonContent.Create(body);
                //Console.WriteLine("body: " + JsonConvert.SerializeObject(body));
       
            var response = await _client.SendAsync(requestMessage);
            token =  ProcessResponse<AuthData>(response).Content.access_token;
        }


        using (var requestMessage = new HttpRequestMessage(method, ApiPrefix + uri))
        {
            Console.WriteLine("method: " + method);
            Console.WriteLine("url: " + ApiPrefix + uri);
           // SetAuthHeaders(requestMessage);

            requestMessage.Headers.Add("Authorization", "Bearer " + token);
            if (body != null)
            {
                requestMessage.Content = JsonContent.Create(body);
                Console.WriteLine("body: " + JsonConvert.SerializeObject(body));
            }
            var response = await _client.SendAsync(requestMessage);
            return ProcessResponse<T>(response);
        }
    }

    public class AuthData2
    {
        public string grant_type { get; set; }
        public string username { get; set; }
        public string password { get; set; }
    }

    public class AuthData
    {
        [JsonProperty("access_token")]
        public string access_token { get; set; }
    }


    private void SetAuthHeaders(HttpRequestMessage requestMessage)
    {
        //AddHeader("Authorization", "Bearer eyJhbGciOiJSU0EtT0FFUCIsImVuYyI6IkEyNTZDQkMtSFM1MTIiLCJraWQiOiI5QkM3MUY5QzM1OEE2NUE3MTA4OEQyNzQ3QUNCOTdDRUE2MjE1MzQzIiwidHlwIjoiYXQrand0IiwiY3R5IjoiSldUIn0.eOGal9CfOHib9CCdAyocVtEHx_wsad8HT0LAmoSq9dpPd9Tqn9g1E8fTPy9WS9Ttkl0B4N7N8ttqeVvfmFSHS9xTcAOoOfBRlf492PAOQrDUjIj9N52p0lhpiDo1HzWXfFaR0ApsEc3bdkaP2plzBoggQgkCpacDXRwaAJG-HzRkptHQNUhSUHnlT4x-6wg984OF4czUgCsP6-CpTtkO-tgrqGro--7FfPcLj3EPDZ82RbTGdsxDNv9GTlBOUSKYiO1VV4trBuxievC28iqukqHZ582m4owBOeKOEQLfaF4ChcXpGsniuTq6hmfkP5GoHHbtPU3seI3eDWkbDSzMkg.3x2lVWSBijLj1iEyB3MUlg.4O5DE2wnX6BUXAnqLoCsrIXcCdYay5LMoDTxgnnt3E0n7z7ihcfn0YGtSS4XgF8stZ68aHSd9yah48NwgTVDX-rOe76thovO7jtma9mKP3iCHJb2vWMnNT320S-ko8YpFRZkfTLZ2YuT0kxAg5Mq5dQlyz-z4Z6wJaQ-ZmlQglRGjFeGaLoK7itiFprOOW5XY8q8SOtiDlSBlQ03P8B4sIIy3PFCEEDZhmKDJMs-YT5YCzk2DT1umNDHwoU8hNU58_a66dy2iUvBEcuztrEvFz5s_tIuLdRSYi5qKZZkV352mDOXbHv_D-S1P7zaPGxqdj5jhdtY3rmhqnUMF_PYj65rUR3QAvbZIaiisCBZUCmRSNJZzt2aSbZigcbXkVNO61KlVebb8ezFu0a70Q9hlJuFU4KranoSRMpdJDjKWKBecCW7X7QgRmZVN7As3U8LUXoU4JHViT09tM6ykRQLXYI7UNoQlJrHHMuGS3tJlGpXKbmLjve4pyg_R5RWcS4o0FeihHEyD2BQIhg85Bop0JLQ1cvWyW_VtE_GwJ7ZisZVj_s24-JCzzsSiwiUdahqxdVEBm1zFdoucI7O5lzbasfAkK5gvCV_vjyKsC8cZKcXUesXJISFxcdD-lXrzUMlT4sh8cyNkhLoxgHggRrSX77zJRRs2ieknoDm4DAeTItli7ocB99pk7TQk_zha-ghZ1vtipbOA71DzVT3j61A6lxUqW-C6UNs2oTYgghgftCnZC7FCdBCTQx2NlyEvBKoW5DZE8FhMpZ8Vhe6XCF6y-zfzG2pOcVwh5WYrxuDSR3X6JXcp4ocxV0I34Z6LBlo0wRR6c9-y2EMtlaFmvTL4KiUSHKXHuhd0TRyYXCX1H_Iq6VR_iRyLBTG4s1sBKyoCnP1ZBlq1unCJepMojGsSt8-faJP8sOey79yJVDcix5vYwkH4ydQBuQz4kFn_36M7Q1GaQtzZ4cNsAYj0wh-Bc1og_0brpVxZYQzvc3gREnz5KCQ0lQvukp7XKYZTSwxpx9uPt8_MZkUD4yuLh6VSeqCUoQlAXIShRsd-pAkqPpp2K0_6PwPAsdBKi-lGQgNGik0bBEAYuPEFhq2Uy8q1qtn4Q1WRikfc-zSiyIybjF-Vj2-EXVDSdCVBnRkt5XwPNyKUGWdYZCLcGU8zN2IKYqDMddjgUctU_Ln5mfbpyk.fBzSsZ1eSX9ph6e_qUL-qGzFzxDol5bs4wboNGRBHss");
        //AddHeader("accept", "*/*");
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
