using Microsoft.AspNetCore.Components;
using Money.ApiClient;

namespace Money.Web.Components.Debts;

public partial class DebtCard : ComponentBase
{
    private bool _open;
    private bool _isExpanded = true;

    [Parameter]
    public Debt Model { get; set; } = null!;

    [Parameter]
    public EventCallback<Debt> OnUpdate { get; set; }

    [Inject]
    private MoneyClient MoneyClient { get; set; } = null!;

    [Inject]
    private ISnackbar SnackbarService { get; set; } = null!;

    // TODO: Подумать как более грамотно сделать
    private string ClassName => Model.IsDeleted
        ? "deleted-operation-card"
        : string.Empty;

    private DebtPayment Payment { get; set; } = new(DateTime.Now, 0, string.Empty);

    private Task Update(Debt entity)
    {
        return OnUpdate.InvokeAsync(entity);
    }

    private Task Delete(Debt entity)
    {
        return Modify(entity, MoneyClient.Debt.Delete, true);
    }

    private Task Restore(Debt entity)
    {
        return Modify(entity, MoneyClient.Debt.Restore, false);
    }

    private async Task Modify(Debt entity, Func<int, Task<ApiClientResponse>> action, bool isDeleted)
    {
        if (entity.Id == null)
        {
            return;
        }

        var result = await action(entity.Id.Value);

        if (result.GetError().ShowMessage(SnackbarService).HasError())
        {
            return;
        }

        entity.IsDeleted = isDeleted;
    }

    private string GetStatusIcon(Debt model)
    {
        var progress = GetPaymentProgress(model);

        return progress switch
        {
            100 => Icons.Material.Filled.CheckCircle,
            > 0 and < 100 => Icons.Material.Filled.Pending,
            _ => Icons.Material.Filled.Warning,
        };
    }

    private Color GetStatusColor(Debt model)
    {
        var progress = GetPaymentProgress(model);

        return progress switch
        {
            100 => Color.Success,
            > 0 and < 100 => Color.Info,
            _ => Color.Error,
        };
    }

    private string GetStatusText(Debt model)
    {
        var progress = GetPaymentProgress(model);

        return progress switch
        {
            100 => "Погашено",
            > 0 and < 100 => "В процессе",
            _ => "Не начато",
        };
    }

    private int GetPaymentProgress(Debt model)
    {
        if (model.Sum == 0)
        {
            return 0;
        }

        return (int)Math.Min(model.PaySum / model.Sum * 100, 100);
    }

    private void ToggleExpand()
    {
        _isExpanded = !_isExpanded;
    }

    private async Task SubmitPayment(Debt model)
    {
        if (Payment.Sum <= 0)
        {
            return;
        }

        // TODO: Pay

        Payment = new(DateTime.Now, 0, string.Empty);
        _open = false;
    }

    private List<DebtPayment> ParsePaymentHistory(string payComment)
    {
        var payments = new List<DebtPayment>();

        if (string.IsNullOrWhiteSpace(payComment))
        {
            return payments;
        }

        var entries = payComment.Split(['\n'], StringSplitOptions.RemoveEmptyEntries);

        foreach (var entry in entries)
        {
            var parts = entry.Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length >= 2)
            {
                var date = DateTime.TryParse(parts[0], out var parsedDate) ? parsedDate : default;
                var sum = decimal.TryParse(parts[1], out var parsedSum) ? parsedSum : 0m;
                var comment = parts.Length > 2 ? parts[2] : string.Empty;

                payments.Add(new(date, sum, comment));
            }
        }

        return payments;
    }

    public class DebtPayment(DateTime date, decimal sum, string comment)
    {
        public string Comment { get; set; } = comment;
        public DateTime? Date { get; set; } = date;
        public decimal Sum { get; set; } = sum;
    }
}
