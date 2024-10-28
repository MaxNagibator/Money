using Microsoft.AspNetCore.Components;
using Money.ApiClient;
using Money.Web.Components;
using System.Globalization;

namespace Money.Web.Pages.Operations;

public partial class Payments
{
    private bool _init;
    private PaymentsFilter _paymentsFilter = null!;
    private PaymentDialog _dialog = null!;

    [CascadingParameter]
    public AppSettings AppSettings { get; set; } = default!;

    [Inject]
    private MoneyClient MoneyClient { get; set; } = default!;

    [Inject]
    private CategoryService CategoryService { get; set; } = default!;

    [Inject]
    private ISnackbar SnackbarService { get; set; } = default!;

    private PaymentClient.PaymentFilterDto? Filter { get; set; }

    private List<PaymentsDay>? PaymentsDays { get; set; }

    private List<Category>? Categories { get; set; }

    private List<Payment>? FilteredPayments { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await Search();
        _init = true;
    }

    private async Task GetCategories()
    {
        Categories ??= await CategoryService.GetCategories();
    }

    private async Task Search(PaymentClient.PaymentFilterDto? filter = null)
    {
        await GetCategories();
        _paymentsFilter.UpdateCategories(Categories!);

        filter ??= _paymentsFilter.GetFilter();
        ApiClientResponse<PaymentClient.Payment[]> apiPayments = await MoneyClient.Payment.Get(filter);

        if (apiPayments.Content == null)
        {
            return;
        }

        Dictionary<int, Category> categoriesDict = Categories!.ToDictionary(x => x.Id!.Value, x => x);

        List<Payment> payments = apiPayments.Content
            .Select(apiPayment => new Payment
            {
                Id = apiPayment.Id,
                Sum = apiPayment.Sum,
                Category = categoriesDict[apiPayment.CategoryId],
                Comment = apiPayment.Comment,
                Date = apiPayment.Date.Date,
                CreatedTaskId = apiPayment.CreatedTaskId,
                Place = apiPayment.Place,
            })
            .ToList();

        PaymentsDays = payments
            .GroupBy(x => x.Date)
            .Select(x => new PaymentsDay
            {
                Date = x.Key,
                Payments = x.ToList(),
            })
            .ToList();

        // TODO: Костыль
        filter.DateTo = filter.DateTo?.AddDays(-1);
        Filter = filter;
        FilteredPayments = payments;
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
