using Microsoft.Extensions.Logging;
using Money.Api.Constracts.Accounts;
using Money.Api.Constracts.Auth;
using Money.Api.Constracts.Categories;
using Money.Api.Constracts.Operations;
using Refit;

namespace Money.ApiClient;

public sealed class MoneyClient
{
    private readonly ILogger<MoneyClient> _logger;
    private readonly HttpClient _httpClient;

    public MoneyClient(HttpClient client, ILogger<MoneyClient> logger)
    {
        _httpClient = client;
        _logger = logger;
        Accounts = RestService.For<IAccountsResource>(client);
        Auth = RestService.For<IAuthResource>(client);
        Categories = RestService.For<ICategoriesResource>(client);
        Operations = RestService.For<IOperationsResource>(client);
    }


    public IAccountsResource Accounts { get; init; }
    public IAuthResource Auth { get; init; }
    public ICategoriesResource Categories { get; init; }
    public IOperationsResource Operations { get; init; }

    public async Task<ApiClientResponse> ResponseHandle(Func<MoneyClient, Task> request)
    {
        try
        {
            await request(this);
            return new ApiClientResponse();
        }
        catch (ApiException e)
        {
            return new ApiClientResponse(e);
        }
    }

    public async Task<ApiClientResponse<T>> ResponseHandle<T>(Func<MoneyClient, Task<T>> request)
        where T : class
    {
        T? responseValue = null;
        try
        {
            responseValue = await request(this);
            return new ApiClientResponse<T>(responseValue, null);
        }
        catch (ApiException e)
        {
            return new ApiClientResponse<T>(responseValue, e);
        }
    }
}