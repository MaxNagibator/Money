namespace Money.Web.Pages.Operations;

public partial class ListOperations(FastOperationService fastOperationService)
{
    private OperationDialog _dialog = null!;

    private List<FastOperation>? _fastOperations;
    private List<OperationsDay>? _operationsDays;

    protected override async Task OnInitializedAsync()
    {
        _fastOperations = await fastOperationService.GetAllAsync();
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

        var days = args.Operations
            .GroupBy(x => x.Date)
            .Select(g =>
            {
                var operationsDay = new OperationsDay
                {
                    Date = g.Key,
                    Operations = [..g],
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
            var i = 0;

            while (i < operationsDays.Count - 1)
            {
                var day = operationsDays[i].Date;
                var nextDay = operationsDays[i + 1].Date.AddDays(1);

                var shift = 0;

                while (day != nextDay)
                {
                    shift++;
                    day = day.AddDays(-1);

                    var operationsDay = new OperationsDay
                    {
                        Date = day,
                        Operations = [],
                    };

                    operationsDay.AddAction = x => AddOperation(x, operationsDay);
                    operationsDays.Insert(i + shift, operationsDay);
                }

                i += shift + 1;
            }
        }
    }

    private void AddNewOperation(Operation operation)
    {
        _operationsDays ??= [];

        var operationDate = operation.Date.Date;
        var operationsDay = _operationsDays.FirstOrDefault(x => x.Date == operationDate);

        if (operationsDay != null)
        {
            operationsDay.Operations.Insert(0, operation);
            return;
        }

        operationsDay = new()
        {
            Date = operationDate,
            Operations = [operation],
        };

        var index = _operationsDays.FindIndex(x => x.Date < operationDate);
        _operationsDays.Insert(index == -1 ? 0 : index, operationsDay);

        StateHasChanged();
    }

    private void AddOperation(Operation operation, OperationsDay day)
    {
        if (operation.Date == day.Date)
        {
            day.Operations.Add(operation);
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
