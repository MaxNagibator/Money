using Money.Business.Enums;

namespace Money.Api.Dto.RegularOperations;

/// <summary>
/// Запрос на сохранение операции.
/// </summary>
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
    /// Наименование.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Идентификатор типа временного шага.
    /// </summary>
    public int TimeTypeId { get; set; }

    /// <summary>
    /// Значение временного шага.
    /// </summary>
    public int? TimeValue { get; set; }

    /// <summary>
    /// Дата начала.
    /// </summary>
    public DateTime DateFrom { get; set; }

    /// <summary>
    /// Дата окончания.
    /// </summary>
    public DateTime? DateTo { get; set; }

    /// <summary>
    /// Преобразует текущую DTO-модель в бизнес-модель.
    /// </summary>
    /// <returns>
    /// Экземпляр <see cref="Business.Models.FastOperation" />, который представляет бизнес-модель.
    /// </returns>
    public RegularOperation ToBusinessModel()
    {
        return new()
        {
            Sum = Sum,
            Name = Name,
            CategoryId = CategoryId,
            Comment = Comment,
            Place = Place,
            DateFrom = DateFrom,
            DateTo = DateTo,
            TimeType = (RegularOperationTimeTypes)TimeTypeId,
            TimeValue = TimeValue,
        };
    }
}
