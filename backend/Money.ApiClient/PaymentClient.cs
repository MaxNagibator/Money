﻿using Microsoft.AspNetCore.WebUtilities;

namespace Money.ApiClient;

public class PaymentClient(MoneyClient apiClient) : ApiClientExecutor(apiClient)
{
    private const string BaseUri = "/Payments";

    protected override string ApiPrefix => "";

    public async Task<ApiClientResponse<Payment[]>> Get(PaymentFilterDto? filter = null)
    {
        return await GetAsync<Payment[]>(ToUriParameters(filter));
    }

    public async Task<ApiClientResponse<Payment>> GetById(int id)
    {
        return await GetAsync<Payment>($"{BaseUri}/{id}");
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

    private string ToUriParameters(PaymentFilterDto? filter)
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
    }

    public class PaymentFilterDto
    {
        /// <summary>
        ///     Дата начала периода.
        /// </summary>
        public DateTime? DateFrom { get; set; }

        /// <summary>
        ///     Дата окончания периода.
        /// </summary>
        public DateTime? DateTo { get; set; }

        /// <summary>
        ///     Список идентификаторов категорий.
        /// </summary>
        public List<int>? CategoryIds { get; set; }

        /// <summary>
        ///     Комментарий.
        /// </summary>
        public string? Comment { get; set; }

        /// <summary>
        ///     Место.
        /// </summary>
        public string? Place { get; set; }
    }

    public class Payment
    {
        public int Id { get; set; }

        public required int CategoryId { get; set; }

        public decimal Sum { get; set; }

        public string? Comment { get; set; }

        public string? Place { get; set; }

        public DateTime Date { get; set; }

        public int? CreatedTaskId { get; set; }
    }
}
