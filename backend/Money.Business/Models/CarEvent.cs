namespace Money.Business.Models;

/// <summary>
/// Авто-событие.
/// </summary>
public class CarEvent
{
    /// <summary>
    /// Идентификатор.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Идентификатор связанного автомобиля.
    /// </summary>
    public int CarId { get; init; }

    /// <summary>
    /// Название.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Тип.
    /// </summary>
    public CarEventTypes Type { get; set; }

    /// <summary>
    /// Дополнительные комментарии.
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Пробег автомобиля.
    /// </summary>
    public int? Mileage { get; set; }

    /// <summary>
    /// Дата.
    /// </summary>
    public DateTime Date { get; set; }
}
