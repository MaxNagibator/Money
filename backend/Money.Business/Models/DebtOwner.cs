namespace Money.Business.Models;

/// <summary>
/// Держатель долга.
/// </summary>
public class DebtOwner
{
    /// <summary>
    /// Идентификатор.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Наименование.
    /// </summary>
    public required string Name { get; set; }
}
