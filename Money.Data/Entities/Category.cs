using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Money.Data.Entities;

[PrimaryKey(nameof(UserId), nameof(Id))]
public class Category
{
    [Key]
    [Column(Order = 1)]
    public int UserId { get; set; }

    [Key]
    [Column(Order = 2)]
    public int Id { get; set; }

    [Required]
    [StringLength(500)]
    public required string Name { get; set; }

    [StringLength(4000)]
    public string? Description { get; set; }

    public int? ParentId { get; set; }

    [StringLength(100)]
    public string? Color { get; set; }

    public int TypeId { get; set; }

    public int? Order { get; set; }

    public DomainUser? User { get; set; }

    public bool IsDeleted { get; set; }
}
