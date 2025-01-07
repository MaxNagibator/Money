namespace Money.ApiClient;

public class DebtClient(MoneyClient apiClient) : ApiClientExecutor(apiClient)
{
    private const string BaseUri = "/Debts";

    protected override string ApiPrefix => "";

    public Task<ApiClientResponse<Debt[]>> Get(int? type = null)
    {
        var paramUri = type == null ? "" : $"?type={type}";
        return GetAsync<Debt[]>($"{BaseUri}{paramUri}");
    }

    public Task<ApiClientResponse<Debt>> GetById(int id)
    {
        return GetAsync<Debt>($"{BaseUri}/{id}");
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

    public Task<ApiClientResponse> PayDebt(int id, PayRequest request)
    {
        return PostAsync($"{BaseUri}/{id}/Pay", request);
    }

    public Task<ApiClientResponse> MergeDebtUsers(int fromUserId, int toUserId)
    {
        return PostAsync($"{BaseUri}/MergeDebtUsers/{fromUserId}/to/{toUserId}");
    }

    public class SaveRequest
    {
        public int TypeId { get; set; }

        public decimal Sum { get; set; }

        public string? Comment { get; set; }

        public required string DebtUserName { get; set; }

        public DateTime Date { get; set; }

        public decimal PaySum { get; set; }

        public string? PayComment { get; set; }

        public bool IsDeleted { get; set; }
    }

    public class Debt : SaveRequest
    {
        public required int Id { get; set; }
    }


    public class PayRequest
    {
        public decimal Sum { get; set; }

        public string? Comment { get; set; }

        public DateTime Date { get; set; }
    }
}
