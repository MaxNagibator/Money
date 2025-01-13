using Microsoft.AspNetCore.Components;

namespace Money.Web.Components.Operations;

public abstract class OperationComponent : ComponentBase, IDisposable
{
    [CascadingParameter]
    public OperationsFilter OperationsFilter { get; set; } = null!;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        OperationsFilter.OnSearch -= OnSearchChanged;
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender == false)
        {
            return;
        }

        OperationsFilter.OnSearch += OnSearchChanged;
    }

    protected abstract void OnSearchChanged(object? sender, OperationSearchEventArgs args);
}
