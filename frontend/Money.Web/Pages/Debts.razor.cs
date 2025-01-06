using Microsoft.AspNetCore.Components;

namespace Money.Web.Pages;

public partial class Debts
{
    private Dictionary<DebtTypes.Value, List<DebtsByUser>> _debts = [];

    [Inject]
    private DebtService DebtService { get; set; } = null!;

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        var debts = await DebtService.GetAllAsync();

        _debts = debts.OrderByDescending(x => x.Date)
            .GroupBy(x => x.Type)
            .OrderByDescending(x => x.Key.Id)
            .ToDictionary(x => x.Key, x => x.GroupBy(d => d.DebtUserName)
                .Select(d => new DebtsByUser(d.Key, d.ToList()))
                .ToList());
    }

    public record DebtsByUser(string UserName, List<Debt> Debts);

    /*private async Task Create()
    {
        var input = new Debt
        {
            Name = string.Empty,
            Category = Category.Empty,
        };

        var created = await ShowDialog("Создать", input);

        if (created == null)
        {
            return;
        }

        _debts.Insert(0, created);
    }

    private Task<Debt?> Update(Debt fastOperation)
    {
        return ShowDialog("Обновить", fastOperation);
    }

    private async Task<Debt?> ShowDialog(string title, Debt fastOperation)
    {
        DialogParameters<DebtDialog> parameters = new()
        {
            { dialog => dialog.Debt, fastOperation },
        };

        var dialog = await DialogService.ShowAsync<DebtDialog>(title, parameters);
        return await dialog.GetReturnValueAsync<Debt>();
    }*/
}
