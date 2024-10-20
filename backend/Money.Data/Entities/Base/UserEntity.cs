using System.ComponentModel.DataAnnotations.Schema;

namespace Money.Data.Entities.Base;

/// <summary>
///     Сущность принадлежащая пользователю.
/// </summary>
[PrimaryKey(nameof(UserId), nameof(Id))]
public abstract class UserEntity
{
    /// <summary>
    ///     Идентификатор пользователя.
    /// </summary>
    [Key]
    [Column(Order = 1)]
    public int UserId { get; set; }

    /// <summary>
    ///     Идентификатор.
    /// </summary>
    [Key]
    [Column(Order = 2)]
    public int Id { get; set; }

    /// <summary>
    ///     Пользователь, которому принадлежит.
    /// </summary>
    public DomainUser? User { get; set; }
}
