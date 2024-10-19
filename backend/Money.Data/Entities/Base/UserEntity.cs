using System.ComponentModel.DataAnnotations.Schema;

namespace Money.Data.Entities.Base;

[PrimaryKey(nameof(UserId), nameof(Id))]
public abstract class UserEntity
{
    [Key]
    [Column(Order = 1)]
    public int UserId { get; set; }

    [Key]
    [Column(Order = 2)]
    public int Id { get; set; }

    /// <summary>
    ///     Пользователь, которому принадлежит.
    /// </summary>
    public DomainUser? User { get; set; }
}
