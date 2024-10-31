using Microsoft.AspNetCore.Components;
using Money.Web.Components;
using System.Globalization;

namespace Money.Web.Layout;

public partial class OperationsLayout
{
    private PaymentsFilter? _paymentsFilter;

    [Inject]
    public NavigationManager NavigationManager { get; set; } = default!;

    private string PeriodString { get; set; } = GetPeriodString(null, null);
    private List<(PaymentTypes.Value type, decimal amount)> Payments { get; } = [];

    protected override void OnInitialized()
    {
        NavigationManager.LocationChanged += async (_, _) =>
        {
            if (_paymentsFilter != null)
            {
                await _paymentsFilter.Search();
            }
        };
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender == false)
        {
            return;
        }

        if (_paymentsFilter != null)
        {
            _paymentsFilter.OnSearch += (_, list) =>
            {
                Payments.Clear();

                foreach (PaymentTypes.Value paymentType in PaymentTypes.Values)
                {
                    decimal? amount = list?.Where(x => x.Category.PaymentType == paymentType).Sum(payment => payment.Sum);
                    Payments.Add((paymentType, amount ?? 0));
                }

                PeriodString = GetPeriodString(_paymentsFilter.DateRange.Start, _paymentsFilter.DateRange.End);
                StateHasChanged();
            };
        }
    }

    private static string GetPeriodString(DateTime? dateFrom, DateTime? dateTo)
    {
        return $"Период с {FormatDate(dateFrom)} "
               + $"по {FormatDate(dateTo)}";

        string FormatDate(DateTime? date) => date?.ToString("d MMMM yyyy", CultureInfo.CurrentCulture) ?? "-";
    }
}
