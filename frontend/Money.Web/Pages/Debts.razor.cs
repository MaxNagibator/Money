using Microsoft.AspNetCore.Components;
using Money.ApiClient;
using Money.Web.Components.Debts;
using System.Globalization;

namespace Money.Web.Pages;

public partial class Debts
{
    private readonly HashSet<int> _selectedDebtIds = [];
    private Dictionary<DebtTypes.Value, List<DebtOwner>> _debts = [];

    private List<DeptType> _types = [];
    private List<DeptType> _filteredTypes = [];
    private List<DebtOwnerMerged>? _owners;

    private string _searchQuery = string.Empty;
    private bool _isMergeOpen;
    private bool _showPaidDebts;

    private CategorySelector? _categorySelector;
    private bool _isForgiveOpen;
    private string _forgiveComment = "Перенос из долгов:";
    private bool _isForgiving;

    public DebtOwnerMerged? OwnerFrom { get; set; }
    public DebtOwnerMerged? OwnerTo { get; set; }

    [Inject]
    private DebtService DebtService { get; set; } = null!;

    [Inject]
    private MoneyClient MoneyClient { get; set; } = null!;

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    [Inject]
    private ISnackbar SnackbarService { get; set; } = null!;

    protected override Task OnInitializedAsync()
    {
        return LoadDebtsAsync();
    }

    private static void ToggleType(DeptType type)
    {
        type.Expanded = !type.Expanded;
    }

    private void SwapOwners()
    {
        (OwnerFrom, OwnerTo) = (OwnerTo, OwnerFrom);
    }

    private async Task OpenMergeAsync(bool state)
    {
        if (_isMergeOpen == false && _owners == null)
        {
            var response = await MoneyClient.Debts.GetOwners();

            _owners = response.Content?
                          .Select(x => new DebtOwnerMerged(x.Id, x.Name ?? "Неназванный держатель"))
                          .ToList()
                      ?? [];
        }

        _isMergeOpen = state;
    }

    private async Task MergeOwnersAsync()
    {
        if (OwnerFrom == null || OwnerTo == null || OwnerFrom.Id == OwnerTo.Id)
        {
            return;
        }

        var response = await MoneyClient.Debts.MergeOwners(OwnerFrom.Id, OwnerTo.Id);

        if (response.IsBadRequest(SnackbarService))
        {
            return;
        }

        SnackbarService.Add("Долги успешно объеденины!", Severity.Success);
        OwnerFrom = OwnerTo = null;
        _isMergeOpen = false;

        await LoadDebtsAsync();
    }

    private Task OpenForgiveAsync(bool state)
    {
        _isForgiveOpen = state;

        if (state == false)
        {
            ClearDebtSelection();
        }

        return Task.CompletedTask;
    }

    private void ToggleDebtSelection(int? debtId)
    {
        if (debtId == null)
        {
            return;
        }

        // TODO: Возможно избыточная проверка
        var debt = _debts.Values
            .SelectMany(x => x)
            .SelectMany(x => x.Debts)
            .FirstOrDefault(x => x.Id == debtId);

        if (debt == null)
        {
            SnackbarService.Add("Долг не найден", Severity.Warning);
            return;
        }

        if (CanDebtBeForgiven(debt) == false)
        {
            SnackbarService.Add("Нельзя простить этот долг", Severity.Warning);
            return;
        }

        if (_selectedDebtIds.Add(debtId.Value) == false)
        {
            _selectedDebtIds.Remove(debtId.Value);
        }
    }

    private bool IsDebtSelected(int? debtId)
    {
        return debtId != null && _selectedDebtIds.Contains(debtId.Value);
    }

    private void ClearDebtSelection()
    {
        _selectedDebtIds.Clear();
    }

    private void SelectAllVisibleDebts()
    {
        var visibleDebtIds = _filteredTypes
            .SelectMany(x => x.Owners)
            .SelectMany(x => x.Debts)
            .Where(CanDebtBeForgiven)
            .Select(debt => debt.Id!.Value);

        foreach (var debtId in visibleDebtIds)
        {
            _selectedDebtIds.Add(debtId);
        }
    }

    private bool CanDebtBeForgiven(Debt debt)
    {
        if (_isForgiveOpen == false)
        {
            return false;
        }

        if (debt.Type.Id != 1)
        {
            return false;
        }

        if (debt.IsDeleted)
        {
            return false;
        }

        if (debt.PaySum >= debt.Sum)
        {
            return false;
        }

        if (debt.Id == null)
        {
            return false;
        }

        return true;
    }

    private async Task ForgiveAsync()
    {
        if (_selectedDebtIds.Count == 0)
        {
            SnackbarService.Add("Выберите долги для прощения", Severity.Warning);
            return;
        }

        if (_categorySelector?.SelectedCategory == null)
        {
            SnackbarService.Add("Выберите категорию расходов", Severity.Warning);
            return;
        }

        if (_categorySelector.SelectedCategory.OperationType.Id != 1)
        {
            SnackbarService.Add("Можно прощать долги только в категории расходов", Severity.Warning);
            return;
        }

        _isForgiving = true;
        StateHasChanged();

        var categoryId = _categorySelector.SelectedCategory.Id!.Value;
        var response = await MoneyClient.Debts.Forgive([.. _selectedDebtIds], categoryId, _forgiveComment);

        if (response.IsBadRequest(SnackbarService))
        {
            return;
        }

        SnackbarService.Add($"Успешно прощено {_selectedDebtIds.Count} долгов", Severity.Success);
        ClearDebtSelection();
        _isForgiveOpen = false;
        await LoadDebtsAsync();

        _isForgiving = false;
        StateHasChanged();
    }

    private async Task LoadDebtsAsync()
    {
        var debts = await DebtService.GetAllAsync(_showPaidDebts);

        var typedDebts = debts.OrderByDescending(x => x.Date)
            .GroupBy(x => x.Type)
            .OrderByDescending(x => x.Key.Id);

        _debts = typedDebts.ToDictionary(x => x.Key, x => x
            .GroupBy(d => d.OwnerName)
            .Select(d => new DebtOwner(d.Key, [..d]))
            .ToList());

        _types = _debts.Select(x => new DeptType(x.Key, x.Value)).ToList();
        _filteredTypes = _types;
    }

    private async Task CreateAsync(DebtTypes.Value? type = null)
    {
        var input = new Debt
        {
            Type = DebtTypes.None,
            OwnerName = string.Empty,
            Date = DateTime.Now.Date,
        };

        var created = await ShowDialogAsync("Создать", input, type);

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
            owners.Insert(0, new(created.OwnerName, [created]));
        }
        else
        {
            owner.Debts.Insert(0, created);
            owners.Sort((x, y) => (y == owner).CompareTo(x == owner));
        }

        UpdateTypes();
    }

    private async Task<Debt?> UpdateAsync(Debt model)
    {
        var updated = await ShowDialogAsync("Обновить", model);

        if (updated == null)
        {
            return null;
        }

        if (updated.Type != model.Type || updated.OwnerName != model.OwnerName)
        {
            Remove(model);
            Add(updated);
        }

        UpdateTypes();
        return updated;
    }

    private void Remove(Debt model)
    {
        if (_debts.TryGetValue(model.Type, out var owners) == false)
        {
            return;
        }

        var owner = owners.Find(x => x.UserName == model.OwnerName);
        owner?.Debts.Remove(model);

        if (owner?.Debts.Count == 0)
        {
            owners.Remove(owner);
        }
    }

    private void Add(Debt model)
    {
        if (_debts.TryGetValue(model.Type, out var owners) == false)
        {
            _debts[model.Type] = owners = [];
        }

        var owner = owners.Find(x => x.UserName == model.OwnerName);

        if (owner == null)
        {
            owners.Add(new(model.OwnerName, [model]));
        }
        else
        {
            owner.Debts.Add(model);
        }
    }

    private void UpdateTypes()
    {
        _types = _debts.Select(x => new DeptType(x.Key, x.Value)).ToList();
        ApplySearchQuery();
    }

    private void ApplySearchQuery()
    {
        _filteredTypes = [];

        foreach (var type in _types)
        {
            var debtOwners = type.Owners
                .Where(owner => string.IsNullOrWhiteSpace(_searchQuery) || owner.Search(_searchQuery))
                .ToList();

            if (debtOwners.Count != 0)
            {
                _filteredTypes.Add(type with
                {
                    Owners = debtOwners,
                });
            }
        }
    }

    private void OnSearchQueryChanged(string searchQuery)
    {
        _searchQuery = searchQuery;
        ApplySearchQuery();
    }

    private Task TogglePaidDebtsAsync(bool toggled)
    {
        _showPaidDebts = toggled;
        return LoadDebtsAsync();
    }

    private async Task<Debt?> ShowDialogAsync(string title, Debt model, DebtTypes.Value? type = null)
    {
        DialogParameters<DebtDialog> parameters = new()
        {
            { dialog => dialog.Model, model },
            { dialog => dialog.RequiredType, type },
        };

        var dialog = await DialogService.ShowAsync<DebtDialog>(title, parameters);
        return await dialog.GetReturnValueAsync<Debt>();
    }
}

public record DebtOwnerMerged(int Id, string UserName);

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

        var spaceIndex = lastRecord.IndexOf(' ', StringComparison.Ordinal);

        if (DateTime.TryParse(lastRecord[..spaceIndex], CultureInfo.InvariantCulture, out var date))
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
