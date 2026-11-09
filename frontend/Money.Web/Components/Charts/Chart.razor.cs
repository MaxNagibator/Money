using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Money.Web.Models.Charts.Config;

namespace Money.Web.Components.Charts;

public sealed partial class Chart(IJSRuntime jsRuntime) : IAsyncDisposable
{
    private readonly string _id = "chart-" + Guid.NewGuid();
    private bool _isRendered;

    [CascadingParameter]
    public AppSettings AppSettings { get; set; } = null!;

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

    public async ValueTask DisposeAsync()
    {
        AppSettings.OnChange -= OnAppSettingsChanged;

        if (_isRendered)
        {
            await jsRuntime.InvokeVoidAsync("moneyChart.destroy", _id);
        }
    }

    protected override void OnInitialized()
    {
        AppSettings.OnChange += OnAppSettingsChanged;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await jsRuntime.InvokeVoidAsync("moneyChart.create", _id, Config);
            _isRendered = true;
        }
    }

    private async void OnAppSettingsChanged(object? sender, EventArgs e)
    {
        await UpdateAsync();
    }
}
