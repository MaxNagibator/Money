using System.ComponentModel.DataAnnotations.Schema;

namespace Money.Data.Entities.Base;

public abstract class UserEntity
{
    [Key]
    [Column(Order = 1)]
    public int UserId { get; set; }

    [Key]
    [Column(Order = 2)]
    public int Id { get; set; }

    /// <summary>
    ///     Пользователь, которому принадлежит платеж.
    /// </summary>
    public DomainUser? User { get; set; }
}
