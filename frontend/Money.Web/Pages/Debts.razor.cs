using Microsoft.AspNetCore.Components;
using Money.Web.Components.Debts;

namespace Money.Web.Pages;

public partial class Debts
{
    private Dictionary<DebtTypes.Value, List<DebtOwner>> _debts = new();

    private List<DeptType> _types = [];
    private List<DeptType> _filteredTypes = [];

    private string _searchQuery = string.Empty;

    [Inject]
    private DebtService DebtService { get; set; } = null!;

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        await LoadDebtsAsync();
    }

    private async Task LoadDebtsAsync()
    {
        var debts = await DebtService.GetAllAsync();

        var typedDebts = debts.OrderByDescending(x => x.Date)
            .GroupBy(x => x.Type)
            .OrderByDescending(x => x.Key.Id);

        _debts = typedDebts.ToDictionary(x => x.Key, x => x
            .GroupBy(d => d.OwnerName)
            .Select(d => new DebtOwner(d.Key, d.ToList()))
            .ToList());

        _types = _debts.Select(x => new DeptType(x.Key, x.Value)).ToList();
        _filteredTypes = _types;
    }

    private async Task Create()
    {
        var input = new Debt
        {
            Type = DebtTypes.None,
            OwnerName = string.Empty,
            Date = DateTime.Now.Date,
        };

        var created = await ShowDialog("Создать", input);

        if (created == null)
        {
            return;
        }

        if (_debts.TryGetValue(created.Type, out var owners) == false)
        {
            _debts[created.Type] = owners = [];
        }

        var owner = owners.Find(x => x.UserName == created.OwnerName);

        if (owner == null)
        {
            owners.Add(new(created.OwnerName, [created]));
        }
        else
        {
            owner.Debts.Add(created);
        }

        UpdateTypes();
    }

    private async Task<Debt?> Update(Debt entity)
    {
        var updated = await ShowDialog("Обновить", entity);

        if (updated == null)
        {
            return null;
        }

        if (updated.Type != entity.Type || updated.OwnerName != entity.OwnerName)
        {
            RemoveDebt(entity);
            AddDebt(updated);
        }

        UpdateTypes();
        return updated;
    }

    private void RemoveDebt(Debt debt)
    {
        if (_debts.TryGetValue(debt.Type, out var owners) == false)
        {
            return;
        }

        var owner = owners.Find(x => x.UserName == debt.OwnerName);
        owner?.Debts.Remove(debt);

        if (owner?.Debts.Count == 0)
        {
            owners.Remove(owner);
        }
    }

    private void AddDebt(Debt debt)
    {
        if (_debts.TryGetValue(debt.Type, out var owners) == false)
        {
            _debts[debt.Type] = owners = [];
        }

        var owner = owners.Find(x => x.UserName == debt.OwnerName);

        if (owner == null)
        {
            owners.Add(new(debt.OwnerName, [debt]));
        }
        else
        {
            owner.Debts.Add(debt);
        }
    }

    private void UpdateTypes()
    {
        _types = _debts.Select(x => new DeptType(x.Key, x.Value)).ToList();
        ApplySearchQuery();
    }

    private void ApplySearchQuery()
    {
        _filteredTypes = string.IsNullOrWhiteSpace(_searchQuery)
            ? _types
            : _types.Where(type => type.Owners
                    .Any(owner => owner.Search(_searchQuery)))
                .ToList();
    }

    private void OnSearchQueryChanged(string searchQuery)
    {
        _searchQuery = searchQuery;
        ApplySearchQuery();
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

    private static bool ToggleType(DeptType type)
    {
        return type.Expanded = !type.Expanded;
    }
}

public record DebtOwner(string UserName, List<Debt> Debts)
{
    public decimal CalculatePercent()
    {
        var total = Debts.Sum(d => d.Sum);
        var paid = Debts.Sum(d => d.PaySum);
        var percentage = total > 0 ? Math.Round(paid / total, 2) : 0;
        return percentage;
    }

    public bool Search(string value)
    {
        return UserName.Contains(value, StringComparison.OrdinalIgnoreCase)
                || Debts.Any(debt => (debt.Comment?.Contains(value, StringComparison.OrdinalIgnoreCase) ?? false)
                || (debt.PayComment?.Contains(value, StringComparison.OrdinalIgnoreCase) ?? false));
    }

    public decimal CalculateRemainder()
    {
        return Debts.Sum(d => d.Sum - d.PaySum);
    }

    public decimal CalculateSum()
    {
        return Debts.Sum(d => d.Sum);
    }

    // TODO: Очень костыльный способ
    public DateTime? GetLastPayDate()
    {
        var comment = Debts.OrderByDescending(d => d.Date).FirstOrDefault()?.PayComment;

        if (string.IsNullOrWhiteSpace(comment))
        {
            return null;
        }

        var lastRecord = comment.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).LastOrDefault();

        if (string.IsNullOrWhiteSpace(lastRecord))
        {
            return null;
        }

        var spaceIndex = lastRecord.IndexOf(' ');

        if (DateTime.TryParse(lastRecord[..spaceIndex], out var date))
        {
            return date;
        }

        return null;
    }
}

public record DeptType(DebtTypes.Value Type, List<DebtOwner> Owners)
{
    public bool Expanded { get; set; } = true;
}
