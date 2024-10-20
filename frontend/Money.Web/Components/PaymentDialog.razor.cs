using Microsoft.AspNetCore.Components;
using Money.ApiClient;
using System.ComponentModel.DataAnnotations;

namespace Money.Web.Components;

public partial class PaymentDialog
{
    private bool _open;
    private bool _isProcessing;

    [Parameter]
    public Payment Payment { get; set; } = default!;

    [Parameter]
    public EventCallback<Payment> OnSubmit { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [SupplyParameterFromForm]
    private InputModel Input { get; set; } = default!;

    [Inject]
    private MoneyClient MoneyClient { get; set; } = default!;

    [Inject]
    private CategoryService CategoryService { get; set; } = default!;

    [Inject]
    private ISnackbar SnackbarService { get; set; } = default!;

    public async Task ToggleOpen()
    {
        _open = !_open;

        if (_open)
        {
            List<Category>? categories = await CategoryService.GetCategories();
            Input = new InputModel
            {
                Category = Payment.Category,
                Comment = Payment.Comment,
                Date = Payment.Date,
                Place = Payment.Place,
                Sum = Payment.Sum,
                // todo обработать, если текущая категория удалена.
                CategoryList = [.. categories!],
            };
        }

        StateHasChanged();
    }

    protected override void OnParametersSet()
    {
        Input = new InputModel();
    }

    private async Task SubmitAsync()
    {
        _isProcessing = true;

        try
        {
            await SaveAsync();
            SnackbarService.Add("Категория успешно сохранена!", Severity.Success);

            Payment.Category = Input.Category;
            Payment.Comment = Input.Comment;
            Payment.Date = Input.Date!.Value;
            Payment.Place = Input.Place;
            Payment.Sum = Input.Sum;

            await OnSubmit.InvokeAsync(Payment);
            ToggleOpen();
        }
        catch (Exception)
        {
            // TODO: добавить логирование ошибки
            SnackbarService.Add("Не удалось сохранить платеж. Пожалуйста, попробуйте еще раз.", Severity.Error);
        }

        _isProcessing = false;
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
            CategoryId = Input.Category.Id!.Value,
            Comment = Input.Comment,
            Date = Input.Date!.Value,
            Sum = Input.Sum,
            Place = Input.Place,
        };
    }

    private sealed class InputModel
    {
        [Required(ErrorMessage = "Категория обязательна.")]
        public Category Category { get; set; }

        public List<Category> CategoryList { get; set; }

        [Required(ErrorMessage = "Требуется сумма.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Сумма должна быть больше нуля.")]
        public decimal Sum { get; set; }

        public string? Comment { get; set; }

        public string? Place { get; set; }

        [Required(ErrorMessage = "Укажите дату.")]
        public DateTime? Date { get; set; }
    }
}
