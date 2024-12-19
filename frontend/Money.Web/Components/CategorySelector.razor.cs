using Microsoft.AspNetCore.Components;

namespace Money.Web.Components;

public partial class CategorySelector : ComponentBase
{
    private bool _isCategoriesTreeOpen;

    [Parameter]
    public SelectionMode SelectionMode { get; set; } = SelectionMode.MultiSelection;

    public Category? SelectedCategory { get; private set; }

    [Inject]
    private CategoryService CategoryService { get; set; } = null!;

    private List<TreeItemData<Category>> InitialTreeItems { get; set; } = [];
    private IReadOnlyCollection<Category>? SelectedCategories { get; set; }

    public List<int>? GetSelectedCategories()
    {
        return SelectedCategories?.Select(x => x.Id!.Value).ToList();
    }

    public void Reset()
    {
        SelectedCategories = null;
        SelectedCategory = null;
        _isCategoriesTreeOpen = false;
    }

    protected override async Task OnInitializedAsync()
    {
        List<Category> categories = await CategoryService.GetAllAsync();
        InitialTreeItems = categories.BuildChildren(null).ToList();
    }

    private string GetHelperText()
    {
        if (SelectionMode != SelectionMode.MultiSelection)
        {
            return SelectedCategory?.Name ?? "Выберите категорию";
        }

        if (SelectedCategories == null || SelectedCategories.Count == 0)
        {
            return "Выберите категории";
        }

        return string.Join(", ", SelectedCategories.Select(x => x.Name));
    }

    private void OnTextChanged(string searchTerm)
    {
        InitialTreeItems.Filter(searchTerm);
    }

    private void ToggleCategoriesTree(bool? isOpen = null)
    {
        isOpen ??= !_isCategoriesTreeOpen;
        _isCategoriesTreeOpen = isOpen.Value;
    }
}
