using Microsoft.AspNetCore.Components;
using Money.Web.Components;

namespace Money.Web.Layout;

public partial class OperationsLayout
{
    private PaymentsFilter? _paymentsFilter;

    [Inject]
    public NavigationManager NavigationManager { get; set; } = default!;

    protected override void OnInitialized()
    {
        NavigationManager.LocationChanged += async (sender, args) =>
        {
            await (_paymentsFilter?.Search() ?? Task.CompletedTask);
        };
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            if (_paymentsFilter != null)
            {
                _paymentsFilter.OnSearch += (sender, list) =>
                {
                    StateHasChanged();
                };
            }
        }

        base.OnAfterRender(firstRender);
    }
}
