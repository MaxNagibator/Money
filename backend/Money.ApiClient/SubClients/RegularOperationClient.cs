namespace Money.ApiClient;

public class RegularOperationClient(MoneyClient apiClient) : ApiClientExecutor(apiClient)
{
    private const string BaseUri = "/RegularOperations";

    protected override string ApiPrefix => "";

    public Task<ApiClientResponse<RegularOperation[]>> Get()
    {
        return GetAsync<RegularOperation[]>($"{BaseUri}");
    }

    public Task<ApiClientResponse<RegularOperation>> GetById(int id)
    {
        return GetAsync<RegularOperation>($"{BaseUri}/{id}");
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

        public int TimeTypeId { get; set; }

        public int? TimeValue { get; set; }

        public DateTime DateFrom { get; set; }

        public DateTime? DateTo { get; set; }
    }

    public class RegularOperation : SaveRequest
    {
        public int Id { get; set; }

        public DateTime RunTime { get; set; }
    }
}
