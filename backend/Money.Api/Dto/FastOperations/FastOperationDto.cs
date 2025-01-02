namespace Money.Api.Dto.FastOperations;

/// <summary>
/// Операция.
/// </summary>
public class FastOperationDto
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
    /// Порядок сортировки.
    /// </summary>
    public int? Order { get; set; }

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
    /// Фабричный метод для создания DTO на основе бизнес-модели.
    /// </summary>
    /// <param name="business">Бизнес-модель.</param>
    /// <returns>Новый объект <see cref="FastOperationDto" />.</returns>
    public static FastOperationDto FromBusinessModel(FastOperation business)
    {
        return new()
        {
            Id = business.Id,
            CategoryId = business.CategoryId,
            Sum = business.Sum,
            Comment = business.Comment,
            Place = business.Place,
            Name = business.Name,
            Order = business.Order,
        };
    }
}
