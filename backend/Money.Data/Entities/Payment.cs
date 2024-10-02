using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Money.Data.Entities;

[PrimaryKey(nameof(UserId), nameof(Id))]
public class Payment : IUserEntity
{
    [Key]
    [Column(Order = 1)]
    public int UserId { get; set; }

    [Key]
    [Column(Order = 2)]
    public int Id { get; set; }

    public decimal Sum { get; set; }

    public int? CategoryId { get; set; }

    public int TypeId { get; set; }

    [StringLength(4000)]
    public string Comment { get; set; }

    // [DataType(DataType.Date)] todo разобраться с атрибутом (нужна только дата)
    public DateTime Date { get; set; }

    public int? TaskId { get; set; }

    public int? CreatedTaskId { get; set; }

    public int? PlaceId { get; set; }

    public DomainUser? User { get; set; }

    public bool IsDeleted { get; set; }
}
