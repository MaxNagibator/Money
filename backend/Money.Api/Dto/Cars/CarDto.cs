namespace Money.Api.Dto.Cars;

/// <summary>
/// Машина.
/// </summary>
public class CarDto
{
    /// <summary>
    /// Идентификатор.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Наименование.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Фабричный метод для создания DTO на основе бизнес-модели.
    /// </summary>
    /// <param name="model">Бизнес-модель.</param>
    /// <returns>Новый объект <see cref="CarDto" />.</returns>
    public static CarDto FromBusinessModel(Car model)
    {
        return new()
        {
            Id = model.Id,
            Name = model.Name,
        };
    }
}
