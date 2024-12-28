using Microsoft.AspNetCore.Components;
using Money.ApiClient;

namespace Money.Web.Pages.Operations;

public partial class ListOperations
{
    private OperationDialog _dialog = null!;

    private List<Category>? _categories;
    private List<FastOperation>? _fastOperations;
    private List<OperationsDay>? _operationsDays;

    [Inject]
    private MoneyClient MoneyClient { get; set; } = null!;

    [Inject]
    private CategoryService CategoryService { get; set; } = null!;

    [Inject]
    private FastOperationService FastOperationService { get; set; } = null!;

    [Inject]
    private ISnackbar SnackbarService { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        _categories = await CategoryService.GetAllAsync();
        _fastOperations = await FastOperationService.GetAllAsync();
    }

    protected override void OnSearchChanged(object? sender, OperationSearchEventArgs args)
    {
        if (args.Operations == null)
        {
            StateHasChanged();
            return;
        }

        if (args.ShouldRender == false && _operationsDays != null)
        {
            if (args.AddZeroDays)
            {
                FillZeroDays(_operationsDays);
            }
            else
            {
                _operationsDays = _operationsDays.Where(x => x.Operations.Count != 0).ToList();
            }

            StateHasChanged();
            return;
        }

        _operationsDays = [];

        List<OperationsDay> days = args.Operations
            .GroupBy(x => x.Date)
            .Select(g =>
            {
                OperationsDay operationsDay = new()
                {
                    Date = g.Key,
                    Operations = g.ToList(),
                };

                operationsDay.AddAction = x => AddOperation(x, operationsDay);

                return operationsDay;
            })
            .ToList();

        if (args.AddZeroDays)
        {
            FillZeroDays(days);
        }

        _operationsDays = days;

        StateHasChanged();
        return;

        void FillZeroDays(List<OperationsDay> operationsDays)
        {
            for (int i = 0; i < operationsDays.Count - 1; i++)
            {
                DateTime day = operationsDays[i].Date;
                DateTime nextDay = operationsDays[i + 1].Date.AddDays(1);

                int shift = 0;

                while (day != nextDay)
                {
                    shift++;
                    day = day.AddDays(-1);

                    OperationsDay operationsDay = new()
                    {
                        Date = day,
                        Operations = [],
                    };

                    operationsDay.AddAction = x => AddOperation(x, operationsDay);
                    operationsDays.Insert(i + shift, operationsDay);
                }

                i += shift;
            }
        }
    }

    private async Task Delete(Operation operation)
    {
        await ModifyOperation(operation, MoneyClient.Operation.Delete, true);
    }

    private async Task Restore(Operation operation)
    {
        await ModifyOperation(operation, MoneyClient.Operation.Restore, false);
    }

    private async Task ModifyOperation(Operation operation, Func<int, Task<ApiClientResponse>> action, bool isDeleted)
    {
        if (operation.Id == null)
        {
            return;
        }

        ApiClientResponse result = await action(operation.Id.Value);

        if (result.GetError().ShowMessage(SnackbarService).HasError())
        {
            return;
        }

        operation.IsDeleted = isDeleted;
    }

    private void AddNewOperation(Operation operation)
    {
        _operationsDays ??= [];

        DateTime operationDate = operation.Date.Date;
        OperationsDay? operationsDay = _operationsDays.FirstOrDefault(x => x.Date == operationDate);

        if (operationsDay != null)
        {
            operationsDay.Operations.Insert(0, operation);
            return;
        }

        operationsDay = new OperationsDay
        {
            Date = operationDate,
            Operations = [operation],
        };

        int index = _operationsDays.FindIndex(x => x.Date < operationDate);
        _operationsDays.Insert(index == -1 ? 0 : index, operationsDay);

        StateHasChanged();
    }

    private void AddOperation(Operation operation, OperationsDay operationsDay)
    {
        if (operation.Date == operationsDay.Date)
        {
            operationsDay.Operations.Add(operation);
        }
        else
        {
            AddNewOperation(operation);
        }
    }

    private void DeleteDay(OperationsDay day)
    {
        _operationsDays?.Remove(day);
        StateHasChanged();
    }
}
