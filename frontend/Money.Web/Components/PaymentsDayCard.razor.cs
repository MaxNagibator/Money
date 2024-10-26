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

    [Parameter]
    public EventCallback<PaymentsDay> OnCanDelete { get; set; }

    private async Task OnSubmit(Payment payment)
    {
        if (payment.Date == PaymentsDay.Date)
        {
            PaymentsDay.Payments.Add(payment);
        }
        else
        {
            await OnAddPayment.InvokeAsync(payment);
        }
    }

    private async Task OnEdit(Payment payment)
    {
        if (payment.Date == PaymentsDay.Date)
        {
            StateHasChanged();
            return;
        }

        PaymentsDay.Payments.Remove(payment);

        if (PaymentsDay.Payments.Count == 0)
        {
            await OnCanDelete.InvokeAsync(PaymentsDay);
        }

        await OnAddPayment.InvokeAsync(payment);
        StateHasChanged();
    }
}
