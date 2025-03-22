namespace Money.ApiClient;

public class CarEventsClient(MoneyClient apiClient) : ApiClientExecutor(apiClient)
{
    private const string BaseUri = "/CarEvents";

    protected override string ApiPrefix => "";

    public Task<ApiClientResponse<CarEvent[]>> Get(int carId)
    {
        return GetAsync<CarEvent[]>($"{BaseUri}/Car/{carId}");
    }

    public Task<ApiClientResponse<CarEvent>> GetById(int id)
    {
        return GetAsync<CarEvent>($"{BaseUri}/{id}");
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
        public int CarId { get; init; }

        public string? Title { get; init; }

        public int TypeId { get; init; }

        public string? Comment { get; init; }

        public int? Mileage { get; init; }

        public DateTime Date { get; init; }
    }

    public class CarEvent : SaveRequest
    {
        public required int Id { get; set; }
    }
}
