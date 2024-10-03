namespace Money.ApiClient;

public class PaymentClient(MoneyClient apiClient) : ApiClientExecutor(apiClient)
{
    private const string BaseUri = "/Payments";

    protected override string ApiPrefix => "";

    public async Task<ApiClientResponse<Category[]>> Get()
    {
        return await GetAsync<Category[]>($"{BaseUri}");
    }

    public async Task<ApiClientResponse<Category>> GetById(int id)
    {
        return await GetAsync<Category>($"{BaseUri}/{id}");
    }

    public async Task<ApiClientResponse<int>> Create(SaveRequest request)
    {
        return await PostAsync<int>(BaseUri, request);
    }

    public async Task<ApiClientResponse> Update(int id, SaveRequest request)
    {
        return await PutAsync($"{BaseUri}/{id}", request);
    }

    public async Task<ApiClientResponse> Delete(int id)
    {
        return await DeleteAsync($"{BaseUri}/{id}");
    }

    public async Task<ApiClientResponse> Restore(int id)
    {
        return await PostAsync($"{BaseUri}/{id}/Restore");
    }

    public class SaveRequest
    {
    }

    public class Category
    {
        public int Id;
    }
}
