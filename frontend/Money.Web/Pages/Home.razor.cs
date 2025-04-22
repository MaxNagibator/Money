using Microsoft.AspNetCore.Components;

namespace Money.Web.Pages;

public partial class Home
{
    private bool _historyVisible;

    private async Task ChangeHistoryVisible()
    {
        _historyVisible = !_historyVisible;
    }
}
