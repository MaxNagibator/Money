using Microsoft.AspNetCore.Components;
using Money.ApiClient;

namespace Money.Web.Pages.Operations;

public partial class ListOperations
{
    private OperationDialog _dialog = null!;

    [Inject]
    private MoneyClient MoneyClient { get; set; } = default!;

    [Inject]
    private CategoryService CategoryService { get; set; } = default!;

    [Inject]
    private ISnackbar SnackbarService { get; set; } = default!;

    private List<Category>? Categories { get; set; }
    private List<OperationsDay>? OperationsDays { get; set; }

    protected override async Task OnInitializedAsync()
    {
        Categories = await CategoryService.GetCategories();
    }

    protected override void OnSearchChanged(object? sender, OperationSearchEventArgs args)
    {
        if (args.Operations == null)
        {
            StateHasChanged();
            return;
        }

        OperationsDays = [];

        Dictionary<DateTime, List<Operation>> days = args.Operations
            .GroupBy(x => x.Date)
            .ToDictionary(x => x.Key, x => x.ToList());

        if (days.Keys.Count != 0)
        {
            DateTime start = days.Keys.Last();
            DateTime end = days.Keys.First();

            for (DateTime date = end; date >= start; date = date.AddDays(-1))
            {
                if (days.TryGetValue(date, out List<Operation>? operations) || args.AddZeroDays)
                {
                    OperationsDays.Add(new OperationsDay
                    {
                        Date = date,
                        Operations = operations ?? [],
                    });
                }
            }
        }

        StateHasChanged();
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
        OperationsDays ??= [];

        DateTime operationDate = operation.Date.Date;
        OperationsDay? operationsDay = OperationsDays.FirstOrDefault(x => x.Date == operationDate);

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

        int index = OperationsDays.FindIndex(x => x.Date < operationDate);
        OperationsDays.Insert(index == -1 ? 0 : index, operationsDay);

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
        OperationsDays?.Remove(day);
        StateHasChanged();
    }
}
