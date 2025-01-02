using Microsoft.AspNetCore.WebUtilities;

namespace Money.ApiClient;

public class OperationClient(MoneyClient apiClient) : ApiClientExecutor(apiClient)
{
    private const string BaseUri = "/Operations";

    protected override string ApiPrefix => "";

    public Task<ApiClientResponse<Operation[]>> Get(OperationFilterDto? filter = null)
    {
        return GetAsync<Operation[]>(ToUriParameters(filter));
    }

    public Task<ApiClientResponse<Operation>> GetById(int id)
    {
        return GetAsync<Operation>($"{BaseUri}/{id}");
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

    public Task<ApiClientResponse<Operation[]>> UpdateBatch(UpdateOperationsBatchRequest request)
    {
        return PostAsync<Operation[]>($"{BaseUri}/UpdateBatch", request);
    }

    public Task<ApiClientResponse<string[]>> GetPlaces(int offset, int count, string? name = null, CancellationToken token = default)
    {
        var postfixUri = string.IsNullOrWhiteSpace(name) ? string.Empty : $"/{name}";

        return GetAsync<string[]>($"{BaseUri}/GetPlaces/{offset}/{count}{postfixUri}", token);
    }

    private static string ToUriParameters(OperationFilterDto? filter)
    {
        if (filter == null)
        {
            return BaseUri;
        }

        Dictionary<string, string?> queryParams = new()
        {
            ["dateFrom"] = filter.DateFrom?.ToString("yyyy.MM.dd"),
            ["dateTo"] = filter.DateTo?.ToString("yyyy.MM.dd"),
            ["comment"] = filter.Comment,
            ["categoryIds"] = filter.CategoryIds == null ? null : string.Join(",", filter.CategoryIds),
            ["place"] = filter.Place,
        };

        return QueryHelpers.AddQueryString(BaseUri, queryParams);
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

        /// <summary>
        /// Дата.
        /// </summary>
        public DateTime Date { get; set; }
    }

    public class OperationFilterDto
    {
        /// <summary>
        /// Дата начала периода.
        /// </summary>
        public DateTime? DateFrom { get; set; }

        /// <summary>
        /// Дата окончания периода.
        /// </summary>
        public DateTime? DateTo { get; set; }

        /// <summary>
        /// Список идентификаторов категорий.
        /// </summary>
        public List<int>? CategoryIds { get; set; }

        /// <summary>
        /// Комментарий.
        /// </summary>
        public string? Comment { get; set; }

        /// <summary>
        /// Место.
        /// </summary>
        public string? Place { get; set; }
    }

    public class UpdateOperationsBatchRequest
    {
        public required List<int> OperationIds { get; init; }
        public required int CategoryId { get; init; }
    }

    public class Operation : SaveRequest
    {
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор родительской регулярной задачи.
        /// </summary>
        /// <remarks>
        /// Не null, если операция создана регулярной задачей.
        /// </remarks>
        public int? CreatedTaskId { get; set; }
    }
}
