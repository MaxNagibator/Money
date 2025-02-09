using Money.ApiClient;
using System.Net;

namespace Money.Api.Tests;

public static class ApiClientExtensions
{
    private static readonly HttpStatusCode[] SuccessStatusCodes =
    [
        HttpStatusCode.OK,
        HttpStatusCode.Created,
        HttpStatusCode.NoContent,
    ];

    public static void SetUser(this MoneyClient client, TestUser user)
    {
        client.SetUser(user.Login, user.Password);
    }

    public static ApiClientResponse IsSuccess(this ApiClientResponse response)
    {
        AssertIsSuccessStatusCode(response.Code, response.StringContent);
        return response;
    }

    public static Task<ApiClientResponse> IsSuccess(this Task<ApiClientResponse> responseTask)
    {
        return IsStatus(responseTask, SuccessStatusCodes);
    }

    public static ApiClientResponse<T> IsSuccess<T>(this ApiClientResponse<T> response)
    {
        AssertIsSuccessStatusCode(response.Code, response.StringContent);
        return response;
    }

    public static async Task<ApiClientResponse<T>> IsSuccess<T>(this Task<ApiClientResponse<T>> responseTask)
    {
        var response = await responseTask;
        AssertIsSuccessStatusCode(response.Code, response.StringContent);
        return response;
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
        AssertIsSuccessStatusCode(response.Code, response.StringContent);
        return response.Content;
    }

    private static async Task<ApiClientResponse> IsStatus(this Task<ApiClientResponse> responseTask, params HttpStatusCode[] statusCodes)
    {
        var response = await responseTask;
        Assert.That(response.Code, Is.AnyOf(statusCodes), response.StringContent);
        return response;
    }

    private static void AssertIsSuccessStatusCode(HttpStatusCode statusCode, string? message = null)
    {
        Assert.That(statusCode, Is.AnyOf(SuccessStatusCodes), message);
    }
}
