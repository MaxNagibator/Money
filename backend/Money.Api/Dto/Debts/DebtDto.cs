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

    public int TypeId { get; set; }

    public decimal Sum { get; set; }

    public string? Comment { get; set; }

    public required string DebtUserName { get; set; }

    public DateTime Date { get; set; }

    public decimal PaySum { get; set; }

    public string? PayComment { get; set; }

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
            DebtUserName = business.DebtUserName,
            PaySum = business.PaySum,
            PayComment = business.PayComment,
            TypeId = (int)business.Type,
            IsDeleted = business.IsDeleted,
        };
    }
}
