using System.ComponentModel.DataAnnotations.Schema;

namespace Money.ApiClient;

public class CategoryClient(MoneyClient apiClient) : ApiClientExecutor(apiClient)
{
    protected override string ApiPrefix => "";

    public async Task<ApiClientResponse<Category[]>> Get(int? type = null)
    {
        string paramUri = type == null ? "" : $"?type={type}";
        return await GetAsync<Category[]>($"/Categories{paramUri}");
    }

    public async Task<ApiClientResponse<Category>> GetById(int id)
    {
        return await GetAsync<Category>($"/Categories/{id}");
    }

    public async Task<ApiClientResponse<int>> Create(CreateCategoryRequest request)
    {
        return await PostAsync<int>($"/Categories", request);
    }

    public async Task<ApiClientResponse> Delete(int id)
    {
        return await DeleteAsync($"/Categories/{id}");
    }

    public class CreateCategoryRequest
    {
        public string? Name { get; set; }

        public int? ParentId { get; set; }

        public int? Order { get; set; }

        public string? Color { get; set; }

        public int PaymentTypeId { get; set; }
    }

    public class Category
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public int? ParentId { get; set; }

        public int? Order { get; set; }

        public string? Color { get; set; }

        public int PaymentTypeId { get; set; }
    }
}