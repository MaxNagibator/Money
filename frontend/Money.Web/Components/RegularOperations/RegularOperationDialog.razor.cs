﻿using Microsoft.AspNetCore.Components;
using Money.ApiClient;
using System.ComponentModel.DataAnnotations;

namespace Money.Web.Components.RegularOperations;

public partial class RegularOperationDialog
{
    private readonly DialogOptions _dialogOptions = new()
    {
        CloseButton = true,
        BackdropClick = false,
    };

    private SmartSum _smartSum = null!;

    private bool _isProcessing;

    [CascadingParameter]
    public MudDialogInstance MudDialog { get; set; } = null!;

    [Parameter]
    public Category Category { get; set; } = null!;

    [Parameter]
    public RegularOperation RegularOperation { get; set; } = null!;

    [SupplyParameterFromForm]
    private InputModel Input { get; set; } = InputModel.Empty;

    [Inject]
    private MoneyClient MoneyClient { get; set; } = null!;

    [Inject]
    private PlaceService PlaceService { get; set; } = null!;

    [Inject]
    private CategoryService CategoryService { get; set; } = null!;

    [Inject]
    private ISnackbar SnackbarService { get; set; } = null!;

    protected override async Task OnParametersSetAsync()
    {
        Input = new InputModel
        {
            Category = RegularOperation.Category == Category.Empty ? null : RegularOperation.Category,
            Comment = RegularOperation.Comment,
            Name = RegularOperation.Name,
            Place = RegularOperation.Place,
            DateFrom = RegularOperation.DateFrom,
            DateTo = RegularOperation.DateTo,
            TimeType = RegularOperation.TimeType,
            TimeValue = RegularOperation.TimeValue,
            RunTime = RegularOperation.RunTime,
        };

        MudDialog.SetOptions(_dialogOptions);

        List<Category> categories = await CategoryService.GetAllAsync();

        if (Input.Category == null)
        {
            Input.CategoryList = [.. categories];
        }
        else
        {
            Input.CategoryList = [.. categories.Where(x => x.OperationType == RegularOperation.Category.OperationType)];
        }
    }

    private async Task SubmitAsync()
    {
        _isProcessing = true;

        try
        {
            decimal? sum = await _smartSum.ValidateSumAsync();

            if (sum == null)
            {
                _isProcessing = false;
                SnackbarService.Add("Нераспознано значение в поле 'сумма'.", Severity.Warning);
                return;
            }

            await SaveAsync();
            SnackbarService.Add("Успех!", Severity.Success);

            RegularOperation.Name = Input.Name!;
            RegularOperation.Category = Input.Category ?? throw new MoneyException("Категория операции не может быть null");
            RegularOperation.Sum = sum.Value;
            RegularOperation.Comment = Input.Comment;
            RegularOperation.Place = Input.Place;
            RegularOperation.DateFrom = Input.DateFrom!.Value;
            RegularOperation.DateTo = Input.DateTo;
            RegularOperation.TimeType = Input.TimeType;
            RegularOperation.TimeValue = Input.TimeValue;

            MudDialog.Close(DialogResult.Ok(RegularOperation));
        }
        catch (Exception)
        {
            // TODO: добавить логирование ошибки
            SnackbarService.Add("Ошибка. Пожалуйста, попробуйте еще раз.", Severity.Error);
        }

        _isProcessing = false;
    }

    private async Task SaveAsync()
    {
        RegularOperationClient.SaveRequest saveRequest = CreateSaveRequest();

        if (RegularOperation.Id == null)
        {
            ApiClientResponse<int> result = await MoneyClient.RegularOperation.Create(saveRequest);
            RegularOperation.Id = result.Content;
        }
        else
        {
            await MoneyClient.RegularOperation.Update(RegularOperation.Id.Value, saveRequest);
        }
    }

    private RegularOperationClient.SaveRequest CreateSaveRequest()
    {
        return new RegularOperationClient.SaveRequest
        {
            CategoryId = Input.Category?.Id ?? throw new MoneyException("Идентификатор отсутствует при сохранении операции"),
            Comment = Input.Comment,
            Name = Input.Name!,
            Sum = _smartSum.Sum,
            Place = Input.Place,
            DateFrom = Input.DateFrom!.Value,
            DateTo = Input.DateTo,
            TimeTypeId = 2,//Input.TimeType.Id,
            TimeValue = 1,//Input.TimeValue,
        };
    }

    private Task<IEnumerable<Category?>> SearchCategoryAsync(string? value, CancellationToken token)
    {
        IEnumerable<Category>? categories = string.IsNullOrWhiteSpace(value)
            ? Input.CategoryList
            : Input.CategoryList?.Where(x => x.Name.Contains(value, StringComparison.InvariantCultureIgnoreCase));

        return Task.FromResult(categories ?? [])!;
    }

    private Task<IEnumerable<string?>> SearchPlaceAsync(string? value, CancellationToken token)
    {
        return PlaceService.SearchPlace(value, token)!;
    }

    private void Cancel()
    {
        MudDialog.Cancel();
    }

    private sealed class InputModel
    {
        public static readonly InputModel Empty = new()
        {
            Category = Category.Empty,
        };

        [Required(ErrorMessage = "Заполни меня")]
        public Category? Category { get; set; }

        public List<Category>? CategoryList { get; set; }

        [Required(ErrorMessage = "Заполни меня")]
        public string? Name { get; set; }

        public string? Comment { get; set; }

        public string? Place { get; set; }

        public RegularOperationTimeTypes.Value TimeType { get; set; }

        public int? TimeValue { get; set; }

        [Required(ErrorMessage = "Укажите дату")]
        public DateTime? DateFrom { get; set; }

        public DateTime? DateTo { get; set; }

        public DateTime? RunTime { get; set; }
    }
}