namespace Money.Web.Models;

public class FastOperation
{
    public int? Id { get; set; }

    public required string Name { get; set; }

    public required Category Category { get; set; }

    public decimal Sum { get; set; }

    public string? Comment { get; set; }

    public string? Place { get; set; }

    public int? Order { get; set; }

    /// <summary>
    ///     Удалён.
    /// </summary>
    public bool IsDeleted { get; set; }
}
