using System.Net;
using Newtonsoft.Json;

namespace Money.Api.Tests;

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

            return JsonConvert.DeserializeObject<T>(StringContent);
        }
    }
}
