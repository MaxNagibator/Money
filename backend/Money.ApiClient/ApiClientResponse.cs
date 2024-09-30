using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Money.ApiClient;

public class ApiClientResponse(HttpStatusCode code, string content)
{
    /// <summary>
    ///     HTTP код ответа.
    /// </summary>
    public HttpStatusCode Code { get; } = code;

    /// <summary>
    ///     Код успешный.
    /// </summary>
    public bool IsSuccessStatusCode => (int)Code >= 200 && (int)Code <= 299;

    /// <summary>
    ///     Содержимое ответа в строковом представлении.
    /// </summary>
    public string StringContent { get; } = content;

    public ProblemDetails? GetError()
    {
        ProblemDetails? problemDetails = null;

        if (IsSuccessStatusCode)
        {
            return problemDetails;
        }

        problemDetails = JsonSerializer.Deserialize<ProblemDetails>(StringContent);
        return problemDetails;
    }
}

public class ApiClientResponse<T>(HttpStatusCode code, string content) : ApiClientResponse(code, content)
{
    private readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    /// <summary>
    ///     Содержимое ответа.
    /// </summary>
    public T? Content => DeserializeContent();

    private T? DeserializeContent()
    {
        if (typeof(T) == typeof(string))
        {
            return (T)Convert.ChangeType(StringContent, typeof(T));
        }

        return JsonSerializer.Deserialize<T>(StringContent, _serializerOptions);
    }
}

public class ProblemDetails(string title, int status)
{
    [JsonPropertyName("title")]
    public string Title { get; init; } = title;

    [JsonPropertyName("status")]
    public int Status { get; init; } = status;
}
