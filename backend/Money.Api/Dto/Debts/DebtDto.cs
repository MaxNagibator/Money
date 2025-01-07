namespace Money.Api.Dto.Debts;

/// <summary>
/// Долг.
/// </summary>
public class DebtDto
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
    /// Оплаченная сумма.
    /// </summary>
    public decimal PaySum { get; set; }

    /// <summary>
    /// Комментарии к оплате.
    /// </summary>
    public string? PayComment { get; set; }

    /// <summary>
    /// Удален.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Фабричный метод для создания DTO на основе бизнес-модели.
    /// </summary>
    /// <param name="business">Бизнес-модель.</param>
    /// <returns>Новый объект <see cref="DebtDto" />.</returns>
    public static DebtDto FromBusinessModel(Debt business)
    {
        return new()
        {
            Id = business.Id,
            Sum = business.Sum,
            Comment = business.Comment,
            Date = business.Date,
            OwnerName = business.OwnerName,
            PaySum = business.PaySum,
            PayComment = business.PayComment,
            TypeId = (int)business.Type,
            IsDeleted = business.IsDeleted,
        };
    }
}
