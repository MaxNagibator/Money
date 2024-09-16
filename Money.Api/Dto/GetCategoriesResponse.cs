using Money.Business.Enums;
using Money.Business.Models;

namespace Money.Api.Dto;

public class GetCategoriesResponse
{
    public GetCategoriesResponse(ICollection<PaymentCategory> business)
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

    public CategoryValue[] Categories { get; set; }

    public class CategoryValue
    {
        public int Id { get; set; }

        public required string Name { get; set; }

        public int? ParentId { get; set; }

        public int? Order { get; set; }

        public string? Color { get; set; }

        public PaymentTypes PaymentType { get; set; }
    }
}
