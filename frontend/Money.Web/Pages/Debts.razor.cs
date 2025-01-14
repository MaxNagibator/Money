﻿using Microsoft.AspNetCore.Components;
using Money.Web.Components.Debts;
using System.Globalization;

namespace Money.Web.Pages;

public partial class Debts
{
    private Dictionary<DebtTypes.Value, List<DebtOwner>> _debts = [];

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

    private static void ToggleType(DeptType type)
    {
        type.Expanded = !type.Expanded;
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

    private async Task Create(DebtTypes.Value? type = null)
    {
        var input = new Debt
        {
            Type = DebtTypes.None,
            OwnerName = string.Empty,
            Date = DateTime.Now.Date,
        };

        var created = await ShowDialog("Создать", input, type);

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

    private async Task<Debt?> Update(Debt model)
    {
        var updated = await ShowDialog("Обновить", model);

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

    private async Task<Debt?> ShowDialog(string title, Debt model, DebtTypes.Value? type = null)
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
