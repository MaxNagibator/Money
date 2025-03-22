using Microsoft.AspNetCore.Components;
using Money.ApiClient;
using Money.Web.Components.CarEvents;

namespace Money.Web.Pages;

public partial class Cars
{
    private bool _init;
    private int _activeIndex;
    private bool _isAddCarOpen;
    private List<Car> _cars = [];
    private string _addCarName = string.Empty;

    [Inject]
    private MoneyClient MoneyClient { get; set; } = null!;

    [Inject]
    private CarService CarService { get; set; } = null!;

    [Inject]
    private CarEventService CarEventService { get; set; } = null!;

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    [Inject]
    private ISnackbar SnackbarService { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        var models = await CarService.GetAllAsync();
        _cars = models.ToList();
        await OnIndexChanged(0);
        _init = true;
    }

    private async Task OnIndexChanged(int index)
    {
        _activeIndex = index;

        if (index >= 0 && index < _cars.Count)
        {
            var car = _cars[index];
            car.Events ??= (await CarEventService.GetAllAsync(car.Id!.Value)).ToList();
            StateHasChanged();
        }
    }

    private void OpenCarAdd()
    {
        _isAddCarOpen = !_isAddCarOpen;
    }

    private async Task CreateCar()
    {
        var model = new Car
        {
            Name = _addCarName,
        };

        var result = await CarService.SaveAsync(model);

        if (result.IsFailure)
        {
            SnackbarService.Add(result.Error, Severity.Warning);
            return;
        }

        SnackbarService.Add("Авто успешно сохранено!", Severity.Success);

        _cars.Add(model);
        _activeIndex = _cars.Count - 1;
        _isAddCarOpen = false;
        StateHasChanged();
    }

    private async Task CreateEvent()
    {
        var model = new CarEvent
        {
            Type = CarEventTypes.None,
            Date = DateTime.Now,
        };

        var created = await ShowDialog("Создать", model);

        if (created == null)
        {
            return;
        }

        var car = _cars[_activeIndex];
        car.Events ??= [];
        car.Events.Insert(0, created);
    }

    private async Task Update(CarEvent model)
    {
        await ShowDialog("Обновить", model);
    }

    private async Task<CarEvent?> ShowDialog(string title, CarEvent model)
    {
        DialogParameters<CarEventDialog> parameters = new()
        {
            { dialog => dialog.Model, model },
            { dialog => dialog.CarId, _cars[_activeIndex].Id!.Value },
            { dialog => dialog.LastMillage, _cars[_activeIndex].Events?.Max(x=>x.Mileage) ?? 0 },
        };

        var dialog = await DialogService.ShowAsync<CarEventDialog>(title, parameters);
        return await dialog.GetReturnValueAsync<CarEvent>();
    }
}
