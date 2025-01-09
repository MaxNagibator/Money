namespace Money.ApiClient;

public class DebtClient(MoneyClient apiClient) : ApiClientExecutor(apiClient)
{
    private const string BaseUri = "/Debts";

    protected override string ApiPrefix => "";

    public Task<ApiClientResponse<Debt[]>> Get()
    {
        return GetAsync<Debt[]>(BaseUri);
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

    public Task<ApiClientResponse> Pay(int id, PayRequest request)
    {
        return PostAsync($"{BaseUri}/{id}/Pay", request);
    }

    public Task<ApiClientResponse> MergeOwners(int fromUserId, int toUserId)
    {
        return PostAsync($"{BaseUri}/MergeOwners/{fromUserId}/with/{toUserId}");
    }

    public Task<ApiClientResponse<DebtOwner[]>> GetOwners()
    {
        return GetAsync<DebtOwner[]>($"{BaseUri}/Owners");
    }

    public Task<ApiClientResponse> Forgive(int[] debtIds, int operationCategoryId, string? operationComment)
    {
        var request = new ForgiveRequest
        {
            DebtIds = debtIds,
            OperationCategoryId = operationCategoryId,
            OperationComment = operationComment,
        };
        return PostAsync($"{BaseUri}/Forgive", request);
    }

    public class SaveRequest
    {
        public int TypeId { get; set; }

        public decimal Sum { get; set; }

        public string? Comment { get; set; }

        public required string OwnerName { get; set; }

        public DateTime Date { get; set; }
    }

    public class Debt : SaveRequest
    {
        public required int Id { get; set; }

        public decimal PaySum { get; set; }

        public string? PayComment { get; set; }
    }

    public class PayRequest
    {
        public decimal Sum { get; set; }

        public string? Comment { get; set; }

        public DateTime Date { get; set; }
    }

    public class DebtOwner
    {
        public int Id { get; set; }

        public string? Name { get; set; }
    }

    public class ForgiveRequest
    {
        public required int[] DebtIds { get; init; }

        public int OperationCategoryId { get; init; }

        public string? OperationComment { get; set; }
    }
}
