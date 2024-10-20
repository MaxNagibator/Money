using Microsoft.AspNetCore.Components;
using Money.ApiClient;
using System.ComponentModel.DataAnnotations;

namespace Money.Web.Components;

public partial class PaymentDialog
{
    private bool _isOpen;

    [Parameter]
    public Payment Payment { get; set; } = default!;

    [Parameter]
    public EventCallback<Payment> OnSubmit { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [SupplyParameterFromForm]
    private InputModel Input { get; set; } = InputModel.Empty;

    [Inject]
    private MoneyClient MoneyClient { get; set; } = default!;

    [Inject]
    private CategoryService CategoryService { get; set; } = default!;

    [Inject]
    private ISnackbar SnackbarService { get; set; } = default!;

    public async Task ToggleOpen()
    {
        _isOpen = !_isOpen;

        if (_isOpen)
        {
            List<Category> categories = await CategoryService.GetCategories() ?? [];

            Input = new InputModel
            {
                Category = Payment.Category,
                Comment = Payment.Comment,
                Date = Payment.Date,
                Place = Payment.Place,
                Sum = Payment.Sum,
                // todo обработать, если текущая категория удалена.
                CategoryList = [.. categories.Where(x => x.PaymentType == Payment.Category.PaymentType)],
            };
        }
    }

    private async Task SubmitAsync()
    {
        try
        {
            await SaveAsync();
            SnackbarService.Add("Платеж успешно сохранен!", Severity.Success);

            Payment.Category = Input.Category;
            Payment.Comment = Input.Comment;
            Payment.Date = Input.Date!.Value;
            Payment.Place = Input.Place;
            Payment.Sum = Input.Sum;

            await OnSubmit.InvokeAsync(Payment);
            await ToggleOpen();
        }
        catch (Exception)
        {
            // TODO: добавить логирование ошибки
            SnackbarService.Add("Не удалось сохранить платеж. Пожалуйста, попробуйте еще раз.", Severity.Error);
        }
    }

    private async Task SaveAsync()
    {
        PaymentClient.SaveRequest clientCategory = CreateSaveRequest();

        if (Payment.Id == null)
        {
            ApiClientResponse<int> result = await MoneyClient.Payment.Create(clientCategory);
            Payment.Id = result.Content;
        }
        else
        {
            await MoneyClient.Payment.Update(Payment.Id.Value, clientCategory);
        }
    }

    private PaymentClient.SaveRequest CreateSaveRequest()
    {
        return new PaymentClient.SaveRequest
        {
            CategoryId = Input.Category.Id ?? throw new MoneyException("Идентификатор категории отсутствует при сохранении платежа"),
            Comment = Input.Comment,
            Date = Input.Date!.Value,
            Sum = Input.Sum,
            Place = Input.Place,
        };
    }

    private sealed class InputModel
    {
        public static readonly InputModel Empty = new()
        {
            Category = Category.Empty,
        };

        [Required(ErrorMessage = "Категория обязательна")]
        public required Category Category { get; set; }

        public List<Category>? CategoryList { get; set; }

        [Required(ErrorMessage = "Требуется сумма")]
        [Range(double.MinValue, double.MaxValue, ErrorMessage = "Сумма вне допустимого диапазона")]
        public decimal Sum { get; set; }

        public string? Comment { get; set; }

        public string? Place { get; set; }

        [Required(ErrorMessage = "Укажите дату")]
        public DateTime? Date { get; set; }
    }
}
