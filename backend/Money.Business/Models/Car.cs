namespace Money.Business.Models;

/// <summary>
/// Авто.
/// </summary>
public class Car
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
