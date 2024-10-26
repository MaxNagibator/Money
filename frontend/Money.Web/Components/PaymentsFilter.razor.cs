using Money.ApiClient;

namespace Money.Web.Components;

public partial class PaymentsFilter
{
    private MudPopover _popover = null!;
    private MudTextField<string> _comment = null!;

    private List<TreeItemData<Category>> InitialTreeItems { get; set; } = [];
    private IReadOnlyCollection<Category>? SelectedCategories { get; set; }

    private string? Comment { get; set; }
    private string? Place { get; set; }

    private DateRange DateRange { get; set; } = new();
    private bool ShowDateRange { get; set; }

    public PaymentClient.PaymentFilterDto GetFilter()
    {
        return new PaymentClient.PaymentFilterDto
        {
            CategoryIds = SelectedCategories?.Select(x => x.Id!.Value).ToList(),
            Comment = Comment,
            Place = Place,
            DateFrom = DateRange.Start,
            DateTo = DateRange.End?.AddDays(1),
        };
    }

    public void UpdateCategories(List<Category> categories)
    {
        InitialTreeItems = BuildChildren(categories, null).ToList();
    }

    private static bool IsMatch(string? text, string searchTerm)
    {
        return string.IsNullOrEmpty(text) == false && text.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);
    }

    private static List<TreeItemData<Category>> BuildChildren(List<Category> categories, int? parentId)
    {
        return categories.Where(category => category.ParentId == parentId)
            .Select(child => new TreeItemData<Category>
            {
                Text = child.Name,
                Value = child,
                Children = BuildChildren(categories, child.Id),
            })
            .OrderBy(item => item.Value?.Order == null)
            .ThenBy(item => item.Value?.Order)
            .ThenBy(item => item.Value?.Name)
            .ToList();
    }

    private static bool IsVisible(TreeItemData<Category> treeItemData, string searchTerm)
    {
        if (treeItemData.HasChildren)
        {
            return IsMatch(treeItemData.Text, searchTerm)
                   || treeItemData.Children!.Any(child => IsMatch(child.Text, searchTerm));
        }

        return IsMatch(treeItemData.Text, searchTerm);
    }

    private static void Filter(IEnumerable<TreeItemData<Category>> treeItemData, string text)
    {
        foreach (TreeItemData<Category> itemData in treeItemData)
        {
            if (itemData.HasChildren)
            {
                Filter(itemData.Children!, text);
            }

            itemData.Visible = IsVisible(itemData, text);
        }
    }

    private string GetHelperText()
    {
        if (SelectedCategories == null || SelectedCategories.Count == 0)
        {
            return "Выберите категории";
        }

        return string.Join(", ", SelectedCategories.Select(x => x.Name));
    }

    private void OnTextChanged(string searchTerm)
    {
        Filter(InitialTreeItems, searchTerm);
    }
}
