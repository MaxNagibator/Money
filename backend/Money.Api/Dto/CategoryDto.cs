using System.Diagnostics.CodeAnalysis;

namespace Money.Api.Dto;

/// <summary>
///     Категория платежа.
/// </summary>
[method: SetsRequiredMembers]
public class CategoryDto(Business.Models.PaymentCategory businessModel)
{
    /// <summary>
    ///     Идентификатор категории.
    /// </summary>
    public int Id { get; set; } = businessModel.Id;

    /// <summary>
    ///     Название категории.
    /// </summary>
    public required string Name { get; set; } = businessModel.Name;

    /// <summary>
    ///     Идентификатор родительской категории (если есть).
    /// </summary>
    public int? ParentId { get; set; } = businessModel.ParentId;

    /// <summary>
    ///     Порядок отображения категории.
    /// </summary>
    public int? Order { get; set; } = businessModel.Order;

    /// <summary>
    ///     Цвет категории.
    /// </summary>
    public string? Color { get; set; } = businessModel.Color;

    /// <summary>
    ///     Идентификатор типа платежа.
    /// </summary>
    public required int PaymentTypeId { get; set; } = businessModel.PaymentType;
}
