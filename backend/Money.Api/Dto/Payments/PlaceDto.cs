namespace Money.Api.Dto.Payments;

/// <summary>
///     Место.
/// </summary>
public class PlaceDto
{
    /// <summary>
    ///     Идентификатор.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    ///     Наименование.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    ///     Фабричный метод для создания DTO на основе бизнес-модели.
    /// </summary>
    /// <param name="business">Бизнес-модель.</param>
    /// <returns>Новый объект <see cref="Place" />.</returns>
    public static PlaceDto FromBusinessModel(Place business)
    {
        return new PlaceDto
        {
            Id = business.Id,
            Name = business.Name,
        };
    }
}
