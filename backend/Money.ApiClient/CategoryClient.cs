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

    public async Task<ApiClientResponse<int>> Create(CreateCategoryRequest request)
    {
        return await PostAsync<int>(BaseUri, request);
    }

    public async Task<ApiClientResponse> Update(UpdateCategoryRequest request)
    {
        return await PutAsync(BaseUri, request);
    }

    public async Task<ApiClientResponse> Delete(int id)
    {
        return await DeleteAsync($"{BaseUri}/{id}");
    }

    public class CreateCategoryRequest
    {
        public required string Name { get; set; }

        public required int PaymentTypeId { get; set; }

        public int? ParentId { get; set; }

        public int? Order { get; set; }

        public string? Color { get; set; }
    }

    public class UpdateCategoryRequest : CreateCategoryRequest
    {
        public required int Id { get; set; }
    }

    public class Category
    {
        public required int Id { get; set; }

        public required string Name { get; set; }

        public required int PaymentTypeId { get; set; }

        public int? ParentId { get; set; }

        public int? Order { get; set; }

        public string? Color { get; set; }
    }
}
