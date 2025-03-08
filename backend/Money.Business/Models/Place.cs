namespace Money.Business.Models;

/// <summary>
/// Место операции.
/// </summary>
public class Place
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
