using Microsoft.AspNetCore.Components;
using Money.Web.Components.Debts;

namespace Money.Web.Pages;

public partial class Debts
{
    private Dictionary<DebtTypes.Value, List<DebtUser>> _debts = [];

    [Inject]
    private DebtService DebtService { get; set; } = null!;

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        var debts = await DebtService.GetAllAsync();

        var typedDebts = debts.OrderByDescending(x => x.Date)
            .GroupBy(x => x.Type)
            .OrderByDescending(x => x.Key.Id);

        _debts = typedDebts.ToDictionary(x => x.Key, x => x
            .GroupBy(d => d.DebtUserName)
            .Select(d => new DebtUser(d.Key, d.ToList()))
            .ToList());
    }

    private async Task Create()
    {
        var input = new Debt
        {
            Type = DebtTypes.None,
            DebtUserName = string.Empty,
            Date = DateTime.Now.Date,
        };

        var created = await ShowDialog("Создать", input);

        if (created == null)
        {
            return;
        }

        if (_debts.TryGetValue(created.Type, out var users) == false)
        {
            _debts[created.Type] = users = [];
        }

        var user = users.Find(x => x.UserName == created.DebtUserName);

        if (user == null)
        {
            users.Add(new(created.DebtUserName, [created]));
        }
        else
        {
            user.Debts.Add(created);
        }
    }

    // TODO: Подумать над перемещением долга при изменении типа или пользователя
    private Task<Debt?> Update(Debt entity)
    {
        return ShowDialog("Обновить", entity);
    }

    private async Task<Debt?> ShowDialog(string title, Debt entity)
    {
        DialogParameters<DebtDialog> parameters = new()
        {
            { dialog => dialog.Entity, entity },
        };

        var dialog = await DialogService.ShowAsync<DebtDialog>(title, parameters);
        return await dialog.GetReturnValueAsync<Debt>();
    }
}

public record DebtUser(string UserName, List<Debt> Debts);
