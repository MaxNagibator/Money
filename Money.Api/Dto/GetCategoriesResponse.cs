using Money.Business.Enums;

namespace Money.Api.Dto;

/// <summary>
///     Ответ на запрос получения категорий.
/// </summary>
public class GetCategoriesResponse
{
    /// <summary>
    ///     Инициализирует новый экземпляр класса <see cref="GetCategoriesResponse" />.
    /// </summary>
    /// <param name="business">Список бизнес-моделей категорий платежей.</param>
    public GetCategoriesResponse(ICollection<Business.Models.PaymentCategory> business)
    {
        Categories = business.Select(x => new CategoryValue
            {
                Id = x.Id,
                Name = x.Name,
                Color = x.Color,
                ParentId = x.ParentId,
                Order = x.Order,
                PaymentType = x.PaymentType
            })
            .ToArray();
    }

    /// <summary>
    ///     Категории.
    /// </summary>
    public CategoryValue[] Categories { get; set; }

    /// <summary>
    ///    Категория платежа.
    /// </summary>
    public class CategoryValue
    {
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
        ///     Тип платежа.
        /// </summary>
        public required PaymentTypes PaymentType { get; set; }
    }
}
