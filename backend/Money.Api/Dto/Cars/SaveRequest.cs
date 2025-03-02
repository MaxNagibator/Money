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
    /// Фабричный метод для создания бизнес-модели на основе DTO.
    /// </summary>
    /// <returns>Новый объект <see cref="Car" />.</returns>
    public Car ToBusinessModel()
    {
        return new()
        {
            Name = Name,
        };
    }
}
