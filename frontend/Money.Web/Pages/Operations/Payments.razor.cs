using Microsoft.AspNetCore.Components;
using Money.ApiClient;
using Money.Web.Components;
using System.Globalization;

namespace Money.Web.Pages.Operations;

public partial class Payments
{
    private PaymentDialog _dialog = null!;

    [CascadingParameter]
    public AppSettings AppSettings { get; set; } = default!;

    [CascadingParameter]
    public PaymentsFilter PaymentsFilter { get; set; } = default!;

    public List<Category>? Categories { get; set; }

    private List<Payment>? FilteredPayments { get; set; }

    [Inject]
    private MoneyClient MoneyClient { get; set; } = default!;

    [Inject]
    private CategoryService CategoryService { get; set; } = default!;

    [Inject]
    private ISnackbar SnackbarService { get; set; } = default!;

    private PaymentClient.PaymentFilterDto? Filter { get; set; }

    private List<PaymentsDay>? PaymentsDays { get; set; }

    protected override async Task OnInitializedAsync()
    {
        Categories = await CategoryService.GetCategories();

        PaymentsFilter.OnSearch += (sender, list) =>
        {
            if (list != null)
            {
                PaymentsDays = list
                    .GroupBy(x => x.Date)
                    .Select(x => new PaymentsDay
                    {
                        Date = x.Key,
                        Payments = x.ToList(),
                    })
                    .ToList();

                FilteredPayments = list;
            }

            PaymentClient.PaymentFilterDto filter = PaymentsFilter.GetFilter();
            filter.DateTo = filter.DateTo?.AddDays(-1);
            Filter = filter;
            StateHasChanged();
        };
    }

    private async Task Delete(Payment payment)
    {
        await ModifyPayment(payment, MoneyClient.Payment.Delete, true);
    }

    private async Task Restore(Payment payment)
    {
        await ModifyPayment(payment, MoneyClient.Payment.Restore, false);
    }

    private async Task ModifyPayment(Payment payment, Func<int, Task<ApiClientResponse>> action, bool isDeleted)
    {
        if (payment.Id == null)
        {
            return;
        }

        ApiClientResponse result = await action(payment.Id.Value);

        if (result.GetError().ShowMessage(SnackbarService).HasError())
        {
            return;
        }

        payment.IsDeleted = isDeleted;
    }

    private void AddNewPayment(Payment payment)
    {
        PaymentsDays ??= [];

        DateTime paymentDate = payment.Date.Date;
        PaymentsDay? paymentsDay = PaymentsDays.FirstOrDefault(x => x.Date == paymentDate);

        if (paymentsDay != null)
        {
            paymentsDay.Payments.Insert(0, payment);
            return;
        }

        paymentsDay = new PaymentsDay
        {
            Date = paymentDate,
            Payments = [payment],
        };

        int index = PaymentsDays.FindIndex(x => x.Date < paymentDate);
        PaymentsDays.Insert(index == -1 ? 0 : index, paymentsDay);

        StateHasChanged();
    }

    private void AddPayment(Payment payment, PaymentsDay paymentsDay)
    {
        if (payment.Date == paymentsDay.Date)
        {
            paymentsDay.Payments.Add(payment);
        }
        else
        {
            AddNewPayment(payment);
        }
    }

    private void DeleteDay(PaymentsDay day)
    {
        PaymentsDays?.Remove(day);
        StateHasChanged();
    }

    private string GetPeriodString(DateTime? dateFrom, DateTime? dateTo)
    {
        return $"Период с {FormatDate(dateFrom)} "
               + $"по {FormatDate(dateTo)}";

        string FormatDate(DateTime? date) => date?.ToString("d MMMM yyyy", CultureInfo.CurrentCulture) ?? "-";
    }
}
