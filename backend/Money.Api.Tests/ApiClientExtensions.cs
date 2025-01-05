using Money.ApiClient;
using System.Net;

namespace Money.Api.Tests;

public static class ApiClientExtensions
{
    public static void SetUser(this MoneyClient client, TestUser user)
    {
        client.SetUser(user.Login, user.Password);
    }

    public static ApiClientResponse<T> IsSuccess<T>(this ApiClientResponse<T> response)
    {
        Assert.That(response.Code, Is.AnyOf(HttpStatusCode.OK, HttpStatusCode.Created), response.StringContent);
        return response;
    }

    public static ApiClientResponse IsSuccess(this ApiClientResponse response)
    {
        Assert.That(response.Code, Is.AnyOf(HttpStatusCode.OK, HttpStatusCode.Created));
        return response;
    }

    public static async Task<ApiClientResponse<T>> IsSuccess<T>(this Task<ApiClientResponse<T>> responseTask)
    {
        var response = await responseTask;
        Assert.That(response.Code, Is.AnyOf(HttpStatusCode.OK, HttpStatusCode.Created), response.StringContent);
        return response;
    }

    public static async Task<ApiClientResponse> IsStatus(this Task<ApiClientResponse> responseTask, params HttpStatusCode[] statusCode)
    {
        var response = await responseTask;
        Assert.That(response.Code, Is.AnyOf(statusCode), response.StringContent);
        return response;
    }

    public static Task<ApiClientResponse> IsSuccess(this Task<ApiClientResponse> responseTask)
    {
        return IsStatus(responseTask, HttpStatusCode.OK, HttpStatusCode.Created);
    }

    public static Task<ApiClientResponse> IsBadRequest(this Task<ApiClientResponse> responseTask)
    {
        return IsStatus(responseTask, HttpStatusCode.BadRequest);
    }

    public static Task<ApiClientResponse> IsNotFound(this Task<ApiClientResponse> responseTask)
    {
        return IsStatus(responseTask, HttpStatusCode.NotFound);
    }

    public static async Task<T?> IsSuccessWithContent<T>(this Task<ApiClientResponse<T>> responseTask)
    {
        var response = await responseTask;
        Assert.That(response.Code, Is.AnyOf(HttpStatusCode.OK, HttpStatusCode.Created), response.StringContent);
        return response.Content;
    }
}
