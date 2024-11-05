using Microsoft.AspNetCore.Components;
using Money.Api.Constracts.Categories;
using Money.ApiClient;
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
    public Category Category { get; set; } = default!;

    [CascadingParameter]
    public MudDialogInstance MudDialog { get; set; } = default!;

    [SupplyParameterFromForm]
    private InputModel Input { get; set; } = default!;

    [Inject]
    private MoneyClient MoneyClient { get; set; } = default!;

    [Inject]
    private ISnackbar SnackbarService { get; set; } = default!;

    protected override void OnParametersSet()
    {
        Input = new InputModel
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

        try
        {
            await SaveAsync();
            _ = SnackbarService.Add("Категория успешно сохранена!", Severity.Success);

            Category.Name = Input.Name;
            Category.Order = Input.Order;
            Category.Color = Input.Color;

            MudDialog.Close(DialogResult.Ok(Category));
        }
        catch (Exception)
        {
            // TODO: добавить логирование ошибки
            _ = SnackbarService.Add("Не удалось сохранить категорию. Пожалуйста, попробуйте еще раз.", Severity.Error);
        }

        _isProcessing = false;
    }

    private async Task SaveAsync()
    {
        CategoryDetailsDTO clientCategory = CreateCategoryDetailsDTO();

        if (Category.Id == null)
        {
            Category.Id = await MoneyClient.Categories.CreateAsync(clientCategory);
        }
        else
        {
            await MoneyClient.Categories.UpdateAsync(Category.Id.Value, clientCategory);
        }
    }

    private CategoryDetailsDTO CreateCategoryDetailsDTO()
    {
        return new CategoryDetailsDTO
        {
            Name = Input.Name,
            Order = Input.Order,
            Color = Input.Color,
            ParentId = Category.ParentId,
            OperationTypeId = Category.OperationType.Id,
        };
    }

    private void Cancel()
    {
        MudDialog.Cancel();
    }

    private sealed class InputModel
    {
        [Display(Name = "Наименование")] // todo подумать как красивее
        [Required(ErrorMessage = "Необходимо указать наименование")]
        public required string Name { get; set; }

        public int? Order { get; set; }

        public string? Color { get; set; }
    }
}
