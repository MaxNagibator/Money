namespace Money.Api.Dto.Cars;

/// <summary>
/// Запрос на сохранение авто.
/// </summary>
public class SaveRequest
{
    /// <summary>
    /// Название.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Преобразует текущую DTO-модель в бизнес-модель.
    /// </summary>
    /// <returns>
    /// Экземпляр <see cref="Car" />, который представляет бизнес-модель.
    /// </returns>
    public Car ToBusinessModel()
    {
        return new()
        {
            Name = Name,
        };
    }
}
