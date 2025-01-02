using Money.Api.Extensions;

namespace Money.Api.Dto.Operations;

/// <summary>
/// Фильтр для операций.
/// </summary>
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
    /// Список идентификаторов категорий (цифры через запятую. Пример: "1,2,5").
    /// </summary>
    public string? CategoryIds { get; set; }

    /// <summary>
    /// Комментарий.
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Место.
    /// </summary>
    public string? Place { get; set; }

    public OperationFilter ToBusinessModel()
    {
        return new()
        {
            CategoryIds = CategoryIds.ParseIds(),
            Comment = Comment,
            Place = Place,
            DateFrom = DateFrom,
            DateTo = DateTo,
        };
    }
}
