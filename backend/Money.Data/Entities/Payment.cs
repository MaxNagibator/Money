using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Money.Data.Entities;

[PrimaryKey(nameof(UserId), nameof(Id))]
public class Payment : IUserEntity
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
    ///     Сумма.
    /// </summary>
    public decimal Sum { get; set; }

    /// <summary>
    ///     Идентификатор категории.
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    ///     Комментарий.
    /// </summary>
    [StringLength(4000)]
    public string? Comment { get; set; }

    /// <summary>
    ///     Дата.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    ///     Идентификатор регулярной задачи.
    /// </summary>
    /// <remarks>
    ///     В одной таблице хранятся две сущности: платежи (TaskId=null) и регулярные задачи (TaskId!=null).
    /// </remarks>
    public int? TaskId { get; set; }

    /// <summary>
    ///     Идентификатор родительской регулярной задачи.
    /// </summary>
    /// <remarks>
    ///     Не null, если платеж создан регулярной задачей.
    /// </remarks>
    public int? CreatedTaskId { get; set; }

    /// <summary>
    ///     Идентификатор места.
    /// </summary>
    public int? PlaceId { get; set; }

    /// <summary>
    ///     Пользователь, которому принадлежит платеж.
    /// </summary>
    public DomainUser? User { get; set; }

    /// <summary>
    ///     Флаг, указывающий, что платеж был удален.
    /// </summary>
    public bool IsDeleted { get; set; }
}
