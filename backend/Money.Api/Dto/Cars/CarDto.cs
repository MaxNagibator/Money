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
    /// Фабричный метод для создания DTO категории на основе бизнес-модели.
    /// </summary>
    /// <param name="category">Бизнес-модель категории.</param>
    /// <returns>Новый объект <see cref="CarDto" />.</returns>
    public static CarDto FromBusinessModel(Car category)
    {
        return new()
        {
            Id = category.Id,
            Name = category.Name,
        };
    }
}
