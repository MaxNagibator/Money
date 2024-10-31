using Microsoft.AspNetCore.Components;

namespace Money.Web.Components;

public partial class OperationItem
{
    private OperationDialog _dialog = null!;

    [Parameter]
    public required Operation Operation { get; set; }

    [Parameter]
    public EventCallback<Operation> OnEdit { get; set; }

    [Parameter]
    public EventCallback<Operation> OnDelete { get; set; }

    private Category Category => Operation.Category;
}
