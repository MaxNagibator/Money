using Microsoft.AspNetCore.Components;
using Money.ApiClient;
using System.ComponentModel.DataAnnotations;

namespace Money.Web.Components.CarEvents;

public partial class CarEventDialog
{
    private readonly DialogOptions _dialogOptions = new()
    {
        CloseButton = true,
        BackdropClick = false,
    };

    private bool _isProcessing;

    [CascadingParameter]
    public IMudDialogInstance MudDialog { get; set; } = null!;

    [Parameter]
    public CarEvent Model { get; set; } = null!;

    [Parameter]
    public int CarId { get; set; }

    [Parameter]
    public int LastMillage { get; set; }

    [SupplyParameterFromForm]
    private InputModel Input { get; set; } = InputModel.Empty;

    [Inject]
    private MoneyClient MoneyClient { get; set; } = null!;

    [Inject]
    private ISnackbar SnackbarService { get; set; } = null!;

    protected override async Task OnParametersSetAsync()
    {
        Input = new()
        {
            Title = Model.Title,
            Comment = Model.Comment,
            Mileage = Model.Mileage,
            Date = Model.Date,
            Type = Model.Type == CarEventTypes.None ? null : Model.Type,
        };

        await MudDialog.SetOptionsAsync(_dialogOptions);
    }

    private async Task SubmitAsync()
    {
        _isProcessing = true;

        try
        {
            await SaveAsync();
            SnackbarService.Add("Успех!", Severity.Success);

            Model.Title = Input.Title;
            Model.Comment = Input.Comment;
            Model.Mileage = Input.Mileage;
            Model.Date = Input.Date!.Value;
            Model.Type = Input.Type!;

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
            var result = await MoneyClient.CarEvents.Create(saveRequest);
            Model.Id = result.Content;
        }
        else
        {
            await MoneyClient.CarEvents.Update(Model.Id.Value, saveRequest);
        }
    }

    private CarEventsClient.SaveRequest CreateSaveRequest()
    {
        return new()
        {
            CarId = CarId,
            Title = Input.Title,
            TypeId = Input.Type!.Id,
            Comment = Input.Comment,
            Mileage = Input.Mileage,
            Date = Input.Date!.Value,
        };
    }

    private void Cancel()
    {
        MudDialog.Cancel();
    }

    private void SelectEventType(CarEventTypes.Value eventType)
    {
        Input.Type = eventType;
    }

    private sealed class InputModel
    {
        public static readonly InputModel Empty = new()
        {
            Type = CarEventTypes.None,
        };

        public string? Title { get; set; }

        [Required(ErrorMessage = "Выбери меня")]
        public required CarEventTypes.Value? Type { get; set; }

        public string? Comment { get; set; }

        public int? Mileage { get; set; }

        [Required(ErrorMessage = "Заполни меня")]
        public DateTime? Date { get; set; }
    }
}
