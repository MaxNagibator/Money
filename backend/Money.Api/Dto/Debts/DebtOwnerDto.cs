namespace Money.Api.Dto.Debts;

/// <summary>
/// Держатель долга.
/// </summary>
public class DebtOwnerDto
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
    /// Фабричный метод для создания DTO на основе бизнес-модели.
    /// </summary>
    /// <param name="business">Бизнес-модель.</param>
    /// <returns>Новый объект <see cref="DebtDto" />.</returns>
    public static DebtOwnerDto FromBusinessModel(DebtOwner business)
    {
        return new()
        {
            Id = business.Id,
            Name = business.Name,
        };
    }
}
