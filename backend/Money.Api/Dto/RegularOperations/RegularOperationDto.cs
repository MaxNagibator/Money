namespace Money.Api.Dto.RegularOperations;

/// <summary>
/// Регулярная операция.
/// </summary>
public class RegularOperationDto
{
    /// <summary>
    /// Идентификатор.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Наименование.
    /// </summary>
    public required string Name { get; set; }

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
    /// Время последнего запуска.
    /// </summary>
    public DateTime? RunTime { get; set; }

    /// <summary>
    /// Фабричный метод для создания DTO на основе бизнес-модели.
    /// </summary>
    /// <param name="business">Бизнес-модель.</param>
    /// <returns>Новый объект <see cref="RegularOperationDto" />.</returns>
    public static RegularOperationDto FromBusinessModel(RegularOperation business)
    {
        return new()
        {
            Id = business.Id,
            CategoryId = business.CategoryId,
            Sum = business.Sum,
            Comment = business.Comment,
            Place = business.Place,
            Name = business.Name,
            DateFrom = business.DateFrom,
            DateTo = business.DateTo,
            TimeTypeId = (int)business.TimeType,
            TimeValue = business.TimeValue,
            RunTime = business.RunTime,
        };
    }
}
