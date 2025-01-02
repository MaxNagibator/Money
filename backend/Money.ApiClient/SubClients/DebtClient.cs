namespace Money.ApiClient;

public class DebtClient(MoneyClient apiClient) : ApiClientExecutor(apiClient)
{
    private const string BaseUri = "/Debts";

    protected override string ApiPrefix => "";

    public async Task<ApiClientResponse<Debt[]>> Get(int? type = null)
    {
        string paramUri = type == null ? "" : $"?type={type}";
        return await GetAsync<Debt[]>($"{BaseUri}{paramUri}");
    }

    public async Task<ApiClientResponse<Debt>> GetById(int id)
    {
        return await GetAsync<Debt>($"{BaseUri}/{id}");
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
        public int TypeId { get; set; }

        public decimal Sum { get; set; }

        public string? Comment { get; set; }

        public required string DebtUserName { get; set; }

        public DateTime Date { get; set; }
    }

    public class Debt : SaveRequest
    {
        public required int Id { get; set; }
    }
}
