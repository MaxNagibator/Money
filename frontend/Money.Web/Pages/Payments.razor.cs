using Microsoft.AspNetCore.Components;
using Money.ApiClient;
using Money.Web.Components;

namespace Money.Web.Pages;

public partial class Payments
{
    private bool _init;
    private PaymentDialog _dialog = null!;
    private MudExpansionPanel _panel;
    private MudPopover _popover;

    [CascadingParameter]
    public AppSettings AppSettings { get; set; } = default!;

    private List<TreeItemData<Category>> InitialTreeItems { get; set; } = [];

    [Inject]
    private MoneyClient MoneyClient { get; set; } = default!;

    [Inject]
    private CategoryService CategoryService { get; set; } = default!;

    [Inject]
    private ISnackbar SnackbarService { get; set; } = default!;

    private List<PaymentsDay>? PaymentsDays { get; set; }

    private List<Category>? Categories { get; set; }

    private IReadOnlyCollection<Category>? FilterCategories { get; set; }
    private string? FilterComment { get; set; }
    private string? FilterPlace { get; set; }
    private DateRange FilterDateRange { get; set; } = new();
    private bool ShowDateRange { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await Search();
        _init = true;
    }

    private string GetHelperText()
    {
        if (FilterCategories == null || FilterCategories.Count == 0)
        {
            return "Выберите категории";
        }

        return string.Join(", ", FilterCategories.Select(x => x.Name));
    }

    private async Task GetCategories()
    {
        Categories ??= await CategoryService.GetCategories();
        InitialTreeItems = BuildChildren(Categories!, null).ToList();
    }

    private void OnTextChanged(string searchTerm)
    {
        Filter(InitialTreeItems, searchTerm);
    }

    private void Filter(IEnumerable<TreeItemData<Category>> treeItemData, string text)
    {
        foreach (TreeItemData<Category> itemData in treeItemData)
        {
            if (itemData.HasChildren)
            {
                Filter(itemData.Children, text);
            }

            itemData.Visible = IsVisible(itemData, text);
        }
    }

    private bool IsVisible(TreeItemData<Category> treeItemData, string searchTerm)
    {
        if (!treeItemData.HasChildren)
        {
            return treeItemData.Text.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);
        }

        return treeItemData.Children.Any(i => i.Text.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
               || treeItemData.Text.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);
    }

    private List<TreeItemData<Category>> BuildChildren(List<Category> categories, int? parentId)
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

    private async Task Search()
    {
        await GetCategories();

        PaymentClient.PaymentFilterDto filter = new()
        {
            CategoryIds = FilterCategories?.Select(x => x.Id!.Value).ToList(),
            Comment = FilterComment,
            Place = FilterPlace,
            DateFrom = FilterDateRange.Start,
            DateTo = FilterDateRange.End?.AddDays(1),
        };

        ApiClientResponse<PaymentClient.Payment[]> apiPayments = await MoneyClient.Payment.Get(filter);

        if (apiPayments.Content == null)
        {
            return;
        }

        Dictionary<int, Category> categoriesDict = Categories!.ToDictionary(x => x.Id!.Value, x => x);

        PaymentsDays = apiPayments.Content
            .Select(apiPayment => new Payment
            {
                Id = apiPayment.Id,
                Sum = apiPayment.Sum,
                Category = categoriesDict[apiPayment.CategoryId],
                Comment = apiPayment.Comment,
                Date = apiPayment.Date.Date,
                CreatedTaskId = apiPayment.CreatedTaskId,
                Place = apiPayment.Place,
            })
            .GroupBy(x => x.Date)
            .Select(x => new PaymentsDay
            {
                Date = x.Key,
                Payments = x.ToList(),
            })
            .ToList();
    }

    private async Task Delete(Payment payment)
    {
        await ModifyPayment(payment, MoneyClient.Payment.Delete, true);
    }

    private async Task Restore(Payment payment)
    {
        await ModifyPayment(payment, MoneyClient.Payment.Restore, false);
    }

    private async Task ModifyPayment(Payment payment, Func<int, Task<ApiClientResponse>> action, bool isDeleted)
    {
        if (payment.Id == null)
        {
            return;
        }

        ApiClientResponse result = await action(payment.Id.Value);

        if (result.GetError().ShowMessage(SnackbarService).HasError())
        {
            return;
        }

        payment.IsDeleted = isDeleted;
    }

    private void AddNewPayment(Payment payment)
    {
        PaymentsDays ??= [];

        DateTime paymentDate = payment.Date.Date;
        PaymentsDay? paymentsDay = PaymentsDays.FirstOrDefault(x => x.Date == paymentDate);

        if (paymentsDay != null)
        {
            paymentsDay.Payments.Insert(0, payment);
            return;
        }

        paymentsDay = new PaymentsDay
        {
            Date = paymentDate,
            Payments = [payment],
        };

        int index = PaymentsDays.FindIndex(x => x.Date < paymentDate);

        PaymentsDays.Insert(index == -1 ? 0 : index, paymentsDay);

        StateHasChanged();
    }

    private void AddPayment(Payment payment, PaymentsDay paymentsDay)
    {
        if (payment.Date == paymentsDay.Date)
        {
            paymentsDay.Payments.Add(payment);
        }
        else
        {
            AddNewPayment(payment);
        }
    }

    private void DeleteDay(PaymentsDay day)
    {
        PaymentsDays?.Remove(day);
        StateHasChanged();
    }
}
