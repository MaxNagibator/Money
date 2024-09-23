using System.Net;
using System.Text.Json;

namespace Money.ApiClient;

public class ApiClientResponse(HttpStatusCode code, string content)
{
    /// <summary>
    ///     HTTP код ответа.
    /// </summary>
    public HttpStatusCode Code { get; set; } = code;

    public string StringContent { get; set; } = content;
}

public class ApiClientResponse<T>(HttpStatusCode code, string content) : ApiClientResponse(code, content)
{
    public T? Content
    {
        get
        {
            if (typeof(T) == typeof(string))
            {
                return (T)Convert.ChangeType(StringContent, typeof(T));
            }

            return JsonSerializer.Deserialize<T>(StringContent, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
        }
    }
}
