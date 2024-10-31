namespace Money.ApiClient;

public class CategoryClient(MoneyClient apiClient) : ApiClientExecutor(apiClient)
{
    private const string BaseUri = "/Categories";

    protected override string ApiPrefix => "";

    public async Task<ApiClientResponse<Category[]>> Get(int? type = null)
    {
        string paramUri = type == null ? "" : $"?type={type}";
        return await GetAsync<Category[]>($"{BaseUri}{paramUri}");
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
        public required string Name { get; set; }

        public required int OperationTypeId { get; set; }

        public int? ParentId { get; set; }

        public int? Order { get; set; }

        public string? Color { get; set; }
    }

    public class Category
    {
        public required int Id { get; set; }

        public required string Name { get; set; }

        public required int OperationTypeId { get; set; }

        public int? ParentId { get; set; }

        public int? Order { get; set; }

        public string? Color { get; set; }
    }
}
