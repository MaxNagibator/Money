using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Money.Web.Models.Charts.Config;

namespace Money.Web.Components.Charts;

public sealed partial class Chart(IJSRuntime jsRuntime) : IAsyncDisposable
{
    private readonly string _id = "chart-" + Guid.NewGuid();
    private bool _isRendered;

    [Parameter]
    public required ChartConfig Config { get; set; }

    public ValueTask UpdateAsync()
    {
        if (_isRendered)
        {
            return jsRuntime.InvokeVoidAsync("moneyChart.update", _id, Config);
        }

        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        if (_isRendered)
        {
            return jsRuntime.InvokeVoidAsync("moneyChart.destroy", _id);
        }

        return ValueTask.CompletedTask;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await jsRuntime.InvokeVoidAsync("moneyChart.create", _id, Config);
            _isRendered = true;
        }
    }
}
