using Money.Business.Models;

namespace Money.Api.Dto.Payments;

/// <summary>
///     Фильтр для платежей.
/// </summary>
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
    ///     Список идентификаторов категорий (цифры через запятую. пример: "1,2,5").
    /// </summary>
    public string? CategoryIds { get; set; }

    /// <summary>
    ///     Комментарий.
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    ///     Место.
    /// </summary>
    public string? Place { get; set; }

    public PaymentFilter ToBusinessModel()
    {
        return new PaymentFilter
        {
            CategoryIds = CategoryIds?.Split(',').Select(int.Parse).ToList(),
            Comment = Comment,
            Place = Place,
            DateFrom = DateFrom,
            DateTo = DateTo,
        };
    }
}
