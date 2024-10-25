using Microsoft.AspNetCore.Components;

namespace Money.Web.Components;

public partial class PaymentsDayCard
{
    private PaymentDialog _dialog = null!;

    [CascadingParameter]
    public required AppSettings Settings { get; set; }

    [Parameter]
    public required PaymentsDay PaymentsDay { get; set; }

    [Parameter]
    public required PaymentTypes.Value[] PaymentTypes { get; set; }

    [Parameter]
    public EventCallback<Payment> OnAddPayment { get; set; }

    [Parameter]
    public EventCallback<Payment> OnRestore { get; set; }

    [Parameter]
    public EventCallback<Payment> OnDelete { get; set; }
}
