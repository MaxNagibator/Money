namespace Money.ApiClient;

public class FastOperationClient(MoneyClient apiClient) : ApiClientExecutor(apiClient)
{
    private const string BaseUri = "/FastOperations";

    protected override string ApiPrefix => "";

    public Task<ApiClientResponse<FastOperation[]>> Get()
    {
        return GetAsync<FastOperation[]>($"{BaseUri}");
    }

    public Task<ApiClientResponse<FastOperation>> GetById(int id)
    {
        return GetAsync<FastOperation>($"{BaseUri}/{id}");
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
        /// <summary>
        /// Идентификатор категории.
        /// </summary>
        public required int CategoryId { get; set; }

        /// <summary>
        /// Сумма.
        /// </summary>
        public decimal Sum { get; set; }

        /// <summary>
        /// Комментарий.
        /// </summary>
        public string? Comment { get; set; }

        /// <summary>
        /// Место.
        /// </summary>
        public string? Place { get; set; }

        public required string Name { get; set; }

        public int? Order { get; set; }
    }

    public class FastOperation : SaveRequest
    {
        public int Id { get; set; }
    }
}
