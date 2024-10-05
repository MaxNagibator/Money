namespace Money.Api.Dto.Payments;

/// <summary>
///     Платеж.
/// </summary>
public class PaymentDto
{
    public int Id { get; set; }

    public required int CategoryId { get; set; }

    public decimal Sum { get; set; }

    public string? Comment { get; set; }

    public string? Place { get; set; }

    public DateTime Date { get; set; }

    public int? CreatedTaskId { get; set; }

    /// <summary>
    ///     Фабричный метод для создания DTO платежа на основе бизнес-модели.
    /// </summary>
    /// <param name="category">Бизнес-модель платежа.</param>
    /// <returns>Новый объект <see cref="PaymentDto" />.</returns>
    public static PaymentDto FromBusinessModel(Business.Models.Payment category)
    {
        return new PaymentDto
        {
            Id = category.Id,
            CategoryId = category.CategoryId,
            Sum = category.Sum,
            Comment = category.Comment,
            Place = category.Place,
            Date = category.Date,
            CreatedTaskId = category.CreatedTaskId,
        };
    }
}
