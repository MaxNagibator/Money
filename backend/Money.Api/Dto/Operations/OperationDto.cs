namespace Money.Api.Dto.Operations;

/// <summary>
/// Операция.
/// </summary>
public class OperationDto
{
    /// <summary>
    /// Идентификатор.
    /// </summary>
    public int Id { get; set; }

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

    /// <summary>
    /// Идентификатор родительской регулярной задачи.
    /// </summary>
    /// <remarks>
    /// Не null, если операция создана регулярной задачей.
    /// </remarks>
    public int? CreatedTaskId { get; set; }

    /// <summary>
    /// Фабричный метод для создания DTO на основе бизнес-модели.
    /// </summary>
    /// <param name="business">Бизнес-модель.</param>
    /// <returns>Новый объект <see cref="OperationDto" />.</returns>
    public static OperationDto FromBusinessModel(Operation business)
    {
        return new()
        {
            Id = business.Id,
            CategoryId = business.CategoryId,
            Sum = business.Sum,
            Comment = business.Comment,
            Place = business.Place,
            Date = business.Date,
            CreatedTaskId = business.CreatedTaskId,
        };
    }
}
