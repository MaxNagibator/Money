using Microsoft.AspNetCore.Components;
using Money.ApiClient;

namespace Money.Web.Pages;

public partial class Payments
{
    private bool _init;

    [CascadingParameter]
    public AppSettings AppSettings { get; set; } = default!;

    [Inject]
    private MoneyClient MoneyClient { get; set; } = default!;

    [Inject]
    private CategoryService CategoryService { get; set; } = default!;

    [Inject]
    private IDialogService DialogService { get; set; } = default!;

    [Inject]
    private ISnackbar SnackbarService { get; set; } = default!;

    private List<PaymentsDay>? PaymentsDays { get; set; }

    private List<Category>? Categories { get; set; }

    protected override async Task OnInitializedAsync()
    {
        ApiClientResponse<PaymentClient.Payment[]> apiPayments = await MoneyClient.Payment.Get();

        if (apiPayments.Content == null)
        {
            return;
        }

        Categories = await CategoryService.GetCategories();
        if (Categories == null)
        {
            return;
        }

        Dictionary<int, Category> categoriesDict = Categories.ToDictionary(x => x.Id!.Value, x => x);

        PaymentsDays = apiPayments.Content
            .Select(apiPayment => new Payment
            {
                Id = apiPayment.Id,
                Sum = apiPayment.Sum,
                Category = categoriesDict[apiPayment.CategoryId],
                Comment = apiPayment.Comment,
                Date = apiPayment.Date,
                CreatedTaskId = apiPayment.CreatedTaskId,
                Place = apiPayment.Place,
            })
            .GroupBy(x => x.Date)
            .Select(x => new PaymentsDay
            {
                Date = x.Key,
                Payments = x.ToList(),
            })
            .ToList();

        _init = true;
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

    private async Task AddNewPayment(Payment payment)
    {
        // todo обработать нормально
        PaymentsDays.First().Payments.Add(payment);
    }
}
