using Microsoft.AspNetCore.Components;
using Money.ApiClient;

namespace Money.Web.Pages;

public partial class Payments
{
    private bool _init;

    [Inject]
    private MoneyClient MoneyClient { get; set; } = default!;

    [Inject]
    private IDialogService DialogService { get; set; } = default!;

    [Inject]
    private ISnackbar SnackbarService { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        ApiClientResponse<PaymentClient.Payment[]> apiPayments = await MoneyClient.Payment.Get();

        if (apiPayments.Content == null)
        {
            return;
        }

        List<Payment> categories = apiPayments.Content
            .Select(apiPayment => new Payment
            {
                Id = apiPayment.Id,
                Sum = apiPayment.Sum,
                CategoryId = apiPayment.CategoryId,
                //ParentId = apiPayment.ParentId,
                //Name = apiPayment.Name,
                //PaymentTypeId = apiPayment.PaymentTypeId,
                //Color = apiPayment.Color,
                //Order = apiPayment.Order,
            })
            .ToList();

        _init = true;
    }

    private async Task Create(DateTime date)
    {
        throw new Exception();
    }

    private async Task Update(Payment category)
    {
        await ShowPaymentDialog("Обновить", category);
    }

    private async Task Delete(Payment category)
    {
        await ModifyPayment(category, MoneyClient.Payment.Delete, true);
    }

    private async Task Restore(Payment category)
    {
        await ModifyPayment(category, MoneyClient.Payment.Restore, false);
    }

    private async Task<Payment?> ShowPaymentDialog(string title, Payment category)
    {
        throw new Exception();
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
}
