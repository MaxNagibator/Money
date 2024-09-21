using Money.Business.Enums;
using System.Diagnostics.CodeAnalysis;

namespace Money.Api.Dto;

/// <summary>
///    Категория платежа.
/// </summary>
public class CategoryDto
{
    [method: SetsRequiredMembers]
    public CategoryDto(Business.Models.PaymentCategory businesModel)
    {
        Id = businesModel.Id;
        Name = businesModel.Name;
        Color = businesModel.Color;
        ParentId = businesModel.ParentId;
        Order = businesModel.Order;
        PaymentTypeId = businesModel.PaymentType;
    }

    /// <summary>
    ///     Идентификатор категории.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    ///     Название категории.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    ///     Идентификатор родительской категории (если есть).
    /// </summary>
    public int? ParentId { get; set; }

    /// <summary>
    ///     Порядок отображения категории.
    /// </summary>
    public int? Order { get; set; }

    /// <summary>
    ///     Цвет категории.
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    ///     Илентификатор типа платежа.
    /// </summary>
    public required int PaymentTypeId { get; set; }
}
