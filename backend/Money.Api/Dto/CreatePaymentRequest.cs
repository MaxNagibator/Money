namespace Money.Api.Dto;

/// <summary>
///     Запрос на создание платежа.
/// </summary>
public class CreatePaymentRequest
{
    /// <summary>
    ///     Цвет категории платежа.
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    ///     Идентификатор родительской категории (если есть).
    /// </summary>
    public int? ParentId { get; set; }

    /// <summary>
    ///     Название категории платежа.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    ///     Порядок отображения категории.
    /// </summary>
    public int? Order { get; set; }

    /// <summary>
    ///     Описание категории платежа.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    ///     Илентификатор типа платежа.
    /// </summary>
    public required int PaymentTypeId { get; set; }

    /// <summary>
    ///     Преобразует текущую модель в бизнес-модель.
    /// </summary>
    /// <returns>Бизнес-модель категории платежа.</returns>
    public Business.Models.PaymentCategory GetBusinessModel()
    {
        return new Business.Models.PaymentCategory
        {
            Color = Color,
            Name = Name,
            Order = Order,
            Description = Description,
            PaymentType = PaymentTypeId,
            ParentId = ParentId
        };
    }
}
