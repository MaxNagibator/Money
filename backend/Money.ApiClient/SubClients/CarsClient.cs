namespace Money.ApiClient;

public class CarsClient(MoneyClient apiClient) : ApiClientExecutor(apiClient)
{
    private const string BaseUri = "/Cars";

    protected override string ApiPrefix => "";

    public Task<ApiClientResponse<Car[]>> Get(int? type = null)
    {
        var paramUri = type == null ? "" : $"?type={type}";
        return GetAsync<Car[]>($"{BaseUri}{paramUri}");
    }

    public Task<ApiClientResponse<Car>> GetById(int id)
    {
        return GetAsync<Car>($"{BaseUri}/{id}");
    }

    public Task<ApiClientResponse<int>> Create(SaveRequest request)
    {
        return PostAsync<int>(BaseUri, request);
    }

    public Task<ApiClientResponse> Update(int id, SaveRequest request)
    {
        return PutAsync($"{BaseUri}/{id}", request);
    }

    public Task<ApiClientResponse> Delete(int id)
    {
        return DeleteAsync($"{BaseUri}/{id}");
    }

    public Task<ApiClientResponse> Restore(int id)
    {
        return PostAsync($"{BaseUri}/{id}/Restore");
    }

    public class SaveRequest
    {
        public required string Name { get; set; }
    }

    public class Car : SaveRequest
    {
        public required int Id { get; set; }
    }
}
