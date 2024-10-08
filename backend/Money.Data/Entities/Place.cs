using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Money.Data.Entities;

[PrimaryKey(nameof(UserId), nameof(Id))]
public class Place : IUserEntity
{
    [Key]
    [Column(Order = 1)]
    public int UserId { get; set; }

    [Key]
    [Column(Order = 2)]
    public int Id { get; set; }

    [StringLength(500)]
    public required string Name { get; set; }

    [StringLength(4000)]
    public string? Description { get; set; }

    public DateTime? LastUsedDate { get; set; }

    public DomainUser? User { get; set; }

    public bool IsDeleted { get; set; }
}
