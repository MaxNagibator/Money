using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Money.ApiClient;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

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

    private Task Update()
    {
        return OnUpdate.InvokeAsync(Model);
    }

    private Task Delete()
    {
        return Modify(MoneyClient.Debt.Delete, true);
    }

    private Task Restore()
    {
        return Modify(MoneyClient.Debt.Restore, false);
    }

    private async Task Modify(Func<int, Task<ApiClientResponse>> action, bool isDeleted)
    {
        if (Model.Id == null)
        {
            return;
        }

        var result = await action(Model.Id.Value);

        if (result.GetError().ShowMessage(SnackbarService).HasError())
        {
            return;
        }

        Model.IsDeleted = isDeleted;
    }

    private void ToggleExpand()
    {
        _isExpanded = !_isExpanded;
    }

    private async Task SubmitPayment(EditContext editContext)
    {
        DebtClient.PayRequest request = new()
        {
            Comment = Payment.Comment,
            Date = Payment.Date!.Value,
            Sum = Payment.Sum,
        };

        var payResponse = await MoneyClient.Debt.Pay(Model.Id!.Value, request);

        if (payResponse.GetError().ShowMessage(SnackbarService).HasError())
        {
            return;
        }

        var response = await MoneyClient.Debt.GetById(Model.Id!.Value);
        var debt = response.Content;

        if (debt != null)
        {
            Model.PaySum = debt.PaySum;
            Model.Sum = debt.Sum;
            Model.PayComment = debt.PayComment;
        }

        Payment = new(DateTime.Now, 0, string.Empty);
        _open = false;
    }

    private string GetStatusIcon()
    {
        var progress = GetPaymentProgress();

        return progress switch
        {
            100 => Icons.Material.Filled.CheckCircle,
            > 0 and < 100 => Icons.Material.Filled.Pending,
            var _ => Icons.Material.Filled.Warning,
        };
    }

    private Color GetStatusColor()
    {
        var progress = GetPaymentProgress();

        return progress switch
        {
            100 => Color.Success,
            > 0 and < 100 => Color.Info,
            var _ => Color.Error,
        };
    }

    private string GetStatusText()
    {
        var progress = GetPaymentProgress();

        return progress switch
        {
            100 => "Погашено",
            > 0 and < 100 => "В процессе",
            var _ => "Не начато",
        };
    }

    private int GetPaymentProgress()
    {
        if (Model.Sum == 0)
        {
            return 0;
        }

        return (int)Math.Min(Model.PaySum / Model.Sum * 100, 100);
    }

    public class DebtPayment(DateTime date, decimal sum, string? comment)
    {
        public string? Comment { get; set; } = comment;

        [Required(ErrorMessage = "Заполни меня")]
        public DateTime? Date { get; set; } = date;

        [Range(1, double.MaxValue, ErrorMessage = "Недопустимое значение")]
        public decimal Sum { get; set; } = sum;

        public static IEnumerable<DebtPayment> ParsePaymentHistory(string payComment)
        {
            if (string.IsNullOrWhiteSpace(payComment))
            {
                yield break;
            }

            var entries = payComment.Split(['\n'], StringSplitOptions.RemoveEmptyEntries);

            foreach (var entry in entries)
            {
                var parts = entry.Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length >= 2)
                {
                    var date = DateTime.TryParse(parts[0], CultureInfo.InvariantCulture, out var parsedDate) ? parsedDate : default;
                    var sum = decimal.TryParse(parts[1], out var parsedSum) ? parsedSum : 0m;
                    var comment = parts.Length > 2 ? parts[2] : string.Empty;

                    yield return new(date, sum, comment);
                }
            }
        }
    }
}
