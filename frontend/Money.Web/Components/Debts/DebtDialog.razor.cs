using Microsoft.AspNetCore.Components;
using Money.ApiClient;
using System.ComponentModel.DataAnnotations;

namespace Money.Web.Components.Debts;

public partial class DebtDialog
{
    private readonly DialogOptions _dialogOptions = new()
    {
        CloseButton = true,
        BackdropClick = false,
    };

    private SmartSum _smartSum = null!;

    private bool _isProcessing;

    [CascadingParameter]
    public MudDialogInstance MudDialog { get; set; } = null!;

    [Parameter]
    public Category Category { get; set; } = null!;

    [Parameter]
    public Debt Entity { get; set; } = null!;

    [SupplyParameterFromForm]
    private InputModel Input { get; set; } = InputModel.Empty;

    [Inject]
    private MoneyClient MoneyClient { get; set; } = null!;

    [Inject]
    private ISnackbar SnackbarService { get; set; } = null!;

    protected override void OnParametersSet()
    {
        Input = new()
        {
            Type = Entity.Type == DebtTypes.None ? null : Entity.Type,
            Comment = Entity.Comment,
            OwnerName = Entity.OwnerName,
            Date = Entity.Date,
        };

        MudDialog.SetOptions(_dialogOptions);
    }

    private async Task SubmitAsync()
    {
        _isProcessing = true;

        try
        {
            var sum = await _smartSum.ValidateSumAsync();

            if (sum == null)
            {
                _isProcessing = false;
                SnackbarService.Add("Нераспознано значение в поле 'сумма'.", Severity.Warning);
                return;
            }

            await SaveAsync();
            SnackbarService.Add("Успех!", Severity.Success);

            Entity.Type = Input.Type!;
            Entity.Sum = _smartSum.Sum;
            Entity.Comment = Input.Comment;
            Entity.OwnerName = Input.OwnerName!;
            Entity.Date = Input.Date!.Value;

            MudDialog.Close(DialogResult.Ok(Entity));
        }
        catch (Exception)
        {
            // TODO: добавить логирование ошибки
            SnackbarService.Add("Ошибка. Пожалуйста, попробуйте еще раз.", Severity.Error);
        }

        _isProcessing = false;
    }

    private async Task SaveAsync()
    {
        var saveRequest = CreateSaveRequest();

        if (Entity.Id == null)
        {
            var result = await MoneyClient.Debt.Create(saveRequest);
            Entity.Id = result.Content;
        }
        else
        {
            await MoneyClient.Debt.Update(Entity.Id.Value, saveRequest);
        }
    }

    private DebtClient.SaveRequest CreateSaveRequest()
    {
        return new()
        {
            TypeId = Input.Type!.Id,
            Sum = _smartSum.Sum,
            Comment = Input.Comment,
            OwnerName = Input.OwnerName!,
            Date = Input.Date!.Value,
        };
    }

    private void Cancel()
    {
        MudDialog.Cancel();
    }

    private sealed class InputModel
    {
        public static readonly InputModel Empty = new();

        [Required(ErrorMessage = "Заполни меня")]
        public DebtTypes.Value? Type { get; set; }

        public string? Comment { get; set; }

        [Required(ErrorMessage = "Заполни меня")]
        public string? OwnerName { get; set; }

        [Required(ErrorMessage = "Заполни меня")]
        public DateTime? Date { get; set; }
    }
}
