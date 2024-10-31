using Microsoft.AspNetCore.Components;
using Money.ApiClient;
using Money.Web.Components;

namespace Money.Web.Pages.Operations;

public partial class ListOperations
{
    private OperationDialog _dialog = null!;

    [CascadingParameter]
    public OperationsFilter OperationsFilter { get; set; } = default!;

    public List<Category>? Categories { get; set; }

    [Inject]
    private MoneyClient MoneyClient { get; set; } = default!;

    [Inject]
    private CategoryService CategoryService { get; set; } = default!;

    [Inject]
    private ISnackbar SnackbarService { get; set; } = default!;

    private List<OperationsDay>? OperationsDays { get; set; }

    protected override async Task OnInitializedAsync()
    {
        Categories = await CategoryService.GetCategories();

        OperationsFilter.OnSearch += (_, list) =>
        {
            if (list != null)
            {
                OperationsDays = list
                    .GroupBy(x => x.Date)
                    .Select(x => new OperationsDay
                    {
                        Date = x.Key,
                        Operations = x.ToList(),
                    })
                    .ToList();
            }

            StateHasChanged();
        };
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
