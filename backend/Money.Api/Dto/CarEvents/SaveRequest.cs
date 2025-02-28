using Money.Business.Enums;

namespace Money.Api.Dto.CarEvents;

/// <summary>
/// Запрос на сохранение авто-события.
/// </summary>
public class SaveRequest
{
    public string? Title { get; init; }

    public int TypeId { get; init; }

    public string? Comment { get; init; }

    public int? Mileage { get; init; }

    public DateTime Date { get; init; }

    /// <summary>
    /// Фабричный метод для создания бизнес-модели на основе DTO.
    /// </summary>
    /// <returns>Новый объект <see cref="CarEvent" />.</returns>
    public CarEvent ToBusinessModel()
    {
        return new()
        {
            Title = Title,
            Type = (CarEventTypes)TypeId,
            Comment = Comment,
            Mileage = Mileage,
            Date = Date,
        };
    }
}
