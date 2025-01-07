using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;

namespace Money.Web.Components;

public partial class CategoryDialog
{
    private readonly DialogOptions _dialogOptions = new()
    {
        CloseButton = true,
        BackdropClick = false,
    };

    private bool _isProcessing;

    [Parameter]
    public Category Category { get; set; } = null!;

    [CascadingParameter]
    public MudDialogInstance MudDialog { get; set; } = null!;

    [SupplyParameterFromForm]
    private InputModel Input { get; set; } = null!;

    [Inject]
    private CategoryService CategoryService { get; set; } = null!;

    [Inject]
    private ISnackbar SnackbarService { get; set; } = null!;

    protected override void OnParametersSet()
    {
        Input = new()
        {
            Name = Category.Name,
            Order = Category.Order,
            Color = Category.Color,
        };

        MudDialog.SetOptions(_dialogOptions);
    }

    private async Task SubmitAsync()
    {
        _isProcessing = true;

        var saved = new Category
        {
            Name = Input.Name,
            Order = Input.Order,
            Color = Input.Color,
            ParentId = Category.ParentId,
            OperationType = Category.OperationType,
        };

        var result = await CategoryService.SaveAsync(saved);

        if (result.IsFailure)
        {
            SnackbarService.Add(result.Error, Severity.Warning);
            return;
        }

        SnackbarService.Add("Категория успешно сохранена!", Severity.Success);

        Category.Name = Input.Name;
        Category.Order = Input.Order;
        Category.Color = Input.Color;

        MudDialog.Close(DialogResult.Ok(Category));

        _isProcessing = false;
    }

    private void Cancel()
    {
        MudDialog.Cancel();
    }

    private sealed class InputModel
    {
        [Display(Name = "Наименование")]
        [Required(ErrorMessage = "Заполни меня")]
        public required string Name { get; set; }

        public int? Order { get; set; }

        public string? Color { get; set; }
    }
}
