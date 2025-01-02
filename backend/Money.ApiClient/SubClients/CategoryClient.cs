namespace Money.ApiClient;

public class CategoryClient(MoneyClient apiClient) : ApiClientExecutor(apiClient)
{
    private const string BaseUri = "/Categories";

    protected override string ApiPrefix => "";

    public Task<ApiClientResponse<Category[]>> Get(int? type = null)
    {
        var paramUri = type == null ? "" : $"?type={type}";
        return GetAsync<Category[]>($"{BaseUri}{paramUri}");
    }

    public Task<ApiClientResponse<Category>> GetById(int id)
    {
        return GetAsync<Category>($"{BaseUri}/{id}");
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

        public required int OperationTypeId { get; set; }

        public int? ParentId { get; set; }

        public int? Order { get; set; }

        public string? Color { get; set; }
    }

    public class Category : SaveRequest
    {
        public required int Id { get; set; }
    }
}
