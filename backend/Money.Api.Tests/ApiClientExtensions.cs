using Money.Api.Tests.TestTools.Entities;
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
        Assert.That(response.Code, Is.EqualTo(HttpStatusCode.OK), response.StringContent);
        return response;
    }

    public static ApiClientResponse IsSuccess(this ApiClientResponse response)
    {
        Assert.That(response.Code, Is.EqualTo(HttpStatusCode.OK));
        return response;
    }

    public static async Task<ApiClientResponse<T>> IsSuccess<T>(this Task<ApiClientResponse<T>> responseTask)
    {
        ApiClientResponse<T> response = await responseTask;
        Assert.That(response.Code, Is.EqualTo(HttpStatusCode.OK), response.StringContent);
        return response;
    }

    public static async Task<ApiClientResponse> IsStatus(this Task<ApiClientResponse> responseTask, HttpStatusCode statusCode)
    {
        ApiClientResponse response = await responseTask;
        Assert.That(response.Code, Is.EqualTo(statusCode), response.StringContent);
        return response;
    }

    public static Task<ApiClientResponse> IsSuccess(this Task<ApiClientResponse> responseTask)
    {
        return IsStatus(responseTask, HttpStatusCode.OK);
    }

    public static Task<ApiClientResponse> IsBadRequest(this Task<ApiClientResponse> responseTask)
    {
        return IsStatus(responseTask, HttpStatusCode.BadRequest);
    }

    public static async Task<T?> IsSuccessWithContent<T>(this Task<ApiClientResponse<T>> responseTask)
    {
        ApiClientResponse<T> response = await responseTask;
        Assert.That(response.Code, Is.EqualTo(HttpStatusCode.OK), response.StringContent);
        return response.Content;
    }
}
