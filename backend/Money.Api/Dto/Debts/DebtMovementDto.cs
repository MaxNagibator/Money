namespace Money.Api.Dto.Debts;

/// <summary>
/// Движение по долговому счёту.
/// </summary>
public class DebtMovementDto
{
    /// <summary>
    /// Идентификатор.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Идентификатор типа.
    /// </summary>
    public int TypeId { get; set; }

    /// <summary>
    /// Сумма.
    /// </summary>
    public decimal Sum { get; set; }

    /// <summary>
    /// Комментарий.
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Имя держателя.
    /// </summary>
    public required string OwnerName { get; set; }

    /// <summary>
    /// Дата возникновения.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Удален.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Фабричный метод для создания DTO на основе бизнес-модели.
    /// </summary>
    /// <param name="business">Бизнес-модель.</param>
    /// <returns>Новый объект <see cref="DebtMovementDto" />.</returns>
    public static DebtMovementDto FromBusinessModel(DebtMovement business)
    {
        return new()
        {
            Id = business.Id,
            Sum = business.Sum,
            Comment = business.Comment,
            Date = business.Date,
            OwnerName = business.OwnerName,
            TypeId = (int)business.Type,
            IsDeleted = business.IsDeleted,
        };
    }
}
