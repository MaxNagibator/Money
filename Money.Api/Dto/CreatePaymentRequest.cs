namespace Money.Api.Dto;

public class CreatePaymentRequest
{
    public string? Color { get; set; }

    public int? ParentId { get; set; }

    public required string Name { get; set; }

    public int? Order { get; set; }

    public string? Description { get; set; }

    public required int PaymentType { get; set; }

    public Business.Models.PaymentCategory GetBusinessModel()
    {
        return new Business.Models.PaymentCategory
        {
            Color = Color,
            Name = Name,
            Order = Order,
            Description = Description,
            PaymentType = PaymentType,
            ParentId = ParentId
        };
    }
}
