namespace Money.Api.Dto.CarEvents;

/// <summary>
/// Авто-событие.
/// </summary>
public class CarEventDto
{
    /// <summary>
    /// Идентификатор.
    /// </summary>
    public int Id { get; init; }

    public string? Title { get; init; }

    public int TypeId { get; init; }

    public string? Comment { get; init; }

    public int? Mileage { get; init; }

    public DateTime Date { get; init; }

    /// <summary>
    /// Фабричный метод для создания DTO на основе бизнес-модели.
    /// </summary>
    /// <param name="model">Бизнес-модель.</param>
    /// <returns>Новый объект <see cref="CarEventDto" />.</returns>
    public static CarEventDto FromBusinessModel(CarEvent model)
    {
        return new()
        {
            Id = model.Id,
            Title = model.Title,
            TypeId = (int)model.Type,
            Comment = model.Comment,
            Mileage = model.Mileage,
            Date = model.Date,
        };
    }
}
