using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Money.Web.Components;

public partial class CategorySelector : ComponentBase, IAsyncDisposable
{
    private bool _isCategoriesTreeOpen;

    private DotNetObjectReference<CategorySelector>? _reference;

    [Parameter]
    public SelectionMode SelectionMode { get; set; } = SelectionMode.MultiSelection;

    public Category? SelectedCategory { get; private set; }

    [Inject]
    private CategoryService CategoryService { get; set; } = null!;

    [Inject]
    private IJSRuntime JsRuntime { get; set; } = null!;

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

    [JSInvokable]
    public Task OnClick()
    {
        return ToggleCategoriesTree(false);
    }

    public async ValueTask DisposeAsync()
    {
        await JsRuntime.InvokeVoidAsync("finalizeClickInterceptor");
        _reference?.Dispose();

        GC.SuppressFinalize(this);
    }

    protected override void OnInitialized()
    {
        _reference = DotNetObjectReference.Create(this);
    }

    protected override async Task OnInitializedAsync()
    {
        var categories = await CategoryService.GetAllAsync();
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

    private async Task ToggleCategoriesTree(bool? isOpen = null)
    {
        isOpen ??= !_isCategoriesTreeOpen;
        _isCategoriesTreeOpen = isOpen.Value;
        StateHasChanged();

        if (isOpen.Value)
        {
            await JsRuntime.InvokeVoidAsync("initializeClickInterceptor", _reference);
        }
        else
        {
            await JsRuntime.InvokeVoidAsync("finalizeClickInterceptor");
        }
    }
}
