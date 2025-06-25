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
    public IMudDialogInstance MudDialog { get; set; } = null!;

    [Parameter]
    public Debt Model { get; set; } = null!;

    [Parameter]
    public DebtTypes.Value? RequiredType { get; set; }

    [SupplyParameterFromForm]
    private InputModel Input { get; set; } = InputModel.Empty;

    [Inject]
    private MoneyClient MoneyClient { get; set; } = null!;

    [Inject]
    private ISnackbar SnackbarService { get; set; } = null!;

    protected override Task OnParametersSetAsync()
    {
        DebtTypes.Value? type;

        if (RequiredType != null)
        {
            type = RequiredType;
        }
        else if (Model.Type != DebtTypes.None)
        {
            type = Model.Type;
        }
        else
        {
            type = null;
        }

        Input = new()
        {
            Type = type,
            Comment = Model.Comment,
            OwnerName = Model.OwnerName,
            Date = Model.Date,
        };

        return MudDialog.SetOptionsAsync(_dialogOptions);
    }

    private async Task SubmitAsync()
    {
        _isProcessing = true;

        try
        {
            var sum = await _smartSum.GetSumAsync();

            if (sum == null)
            {
                _isProcessing = false;
                SnackbarService.Add("Нераспознано значение в поле 'сумма'.", Severity.Warning);
                return;
            }

            await SaveAsync();
            SnackbarService.Add("Успех!", Severity.Success);

            Model.Type = Input.Type!;
            Model.Sum = _smartSum.Sum ?? 0;
            Model.Comment = Input.Comment;
            Model.OwnerName = Input.OwnerName!;
            Model.Date = Input.Date!.Value;

            MudDialog.Close(DialogResult.Ok(Model));
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

        if (Model.Id == null)
        {
            var result = await MoneyClient.Debts.Create(saveRequest);
            Model.Id = result.Content;
        }
        else
        {
            await MoneyClient.Debts.Update(Model.Id.Value, saveRequest);
        }
    }

    private DebtClient.SaveRequest CreateSaveRequest()
    {
        return new()
        {
            TypeId = Input.Type!.Id,
            Sum = _smartSum.Sum ?? 0,
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
