using System.Net;

namespace Money.Api.Tests.ApiClient;

public static class ApiClientExtensions
{
    public static ApiClientResponse<T> IsSuccess<T>(this ApiClientResponse<T> response)
    {
        Assert.That(response.Code, Is.EqualTo(HttpStatusCode.OK), response.StringContent);
        return response;
    }

    public static ApiClientResponse IsSuccess(this ApiClientResponse response)
    {
        Assert.That(response.Code, Is.EqualTo(HttpStatusCode.OK));
        return response;
    }
}
