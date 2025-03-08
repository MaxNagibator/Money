using Money.Business.Enums;

namespace Money.Api.Dto.CarEvents;

/// <summary>
/// Запрос на сохранение авто-события.
/// </summary>
public class SaveRequest
{
    /// <summary>
    /// Название.
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Идентификатор типа.
    /// </summary>
    public int TypeId { get; init; }

    /// <summary>
    /// Идентификатор связанного автомобиля.
    /// </summary>
    public int CarId { get; init; }

    /// <summary>
    /// Дополнительные комментарии.
    /// </summary>
    public string? Comment { get; init; }

    /// <summary>
    /// Пробег автомобиля.
    /// </summary>
    public int? Mileage { get; init; }

    /// <summary>
    /// Дата.
    /// </summary>
    public DateTime Date { get; init; }

    /// <summary>
    /// Фабричный метод для создания бизнес-модели на основе DTO.
    /// </summary>
    /// <returns>Новый объект <see cref="CarEvent" />.</returns>
    public CarEvent ToBusinessModel()
    {
        return new()
        {
            CarId = CarId,
            Title = Title,
            Type = (CarEventTypes)TypeId,
            Comment = Comment,
            Mileage = Mileage,
            Date = Date,
        };
    }
}
