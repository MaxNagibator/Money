using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Money.Api.Data;

[PrimaryKey(nameof(UserId), nameof(Id))]
public class Category
{
    [Key]
    [Column(Order = 1)]
    public int UserId { get; set; }

    [Key]
    [Column(Order = 2)]
    public int Id { get; set; }

    [Column]
    [Required]
    [StringLength(500)]
    public required string Name { get; set; }

    [Column]
    [StringLength(4000)]
    public string? Description { get; set; }

    [Column]
    public int? ParentId { get; set; }

    [Column]
    [StringLength(100)]
    public string? Color { get; set; }

    [Column]
    public int TypeId { get; set; }

    [Column]
    public int? Order { get; set; }

    public required DomainUser User { get; set; }
}
