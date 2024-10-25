using Microsoft.AspNetCore.Components;

namespace Money.Web.Components;

public partial class PaymentItem
{
    private PaymentDialog _dialog = null!;

    [Parameter]
    public required Payment Payment { get; set; }

    [Parameter]
    public EventCallback<Payment> OnEdit { get; set; }

    [Parameter]
    public EventCallback<Payment> OnDelete { get; set; }

    private Category Category => Payment.Category;
}
