using Refit;
using System.Net;
using ProblemDetails = Money.Api.Constracts.ProblemDetails;

namespace Money.ApiClient;

public class ApiClientResponse<T> : ApiClientResponse
{
    public ApiClientResponse(T? response, ApiException? apiException = null)
        : base(apiException)
    {
        Result = response;
    }

    public T? Result { get; }
}


public class ApiClientResponse
{
    public ApiClientResponse(ApiException? apiException = null)
    {
        ApiException = apiException;
    }

    protected readonly ApiException? ApiException;

    public HttpStatusCode Code => ApiException == null ? HttpStatusCode.OK : ApiException.StatusCode;

    public bool IsSuccessStatusCode => ApiException == null;

    public Task<ProblemDetails?> GetProblemDetails()
    {
        // ибегание async для того чтобы лишний раз не разворачивать машину состояний
        return ApiException?.GetContentAsAsync<ProblemDetails>() ??
            Task.FromResult((ProblemDetails?)null);
    }
}
