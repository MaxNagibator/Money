using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Money.Api.Data;

[Table("categories")]
[PrimaryKey(nameof(UserId), nameof(Id))]
public partial class Category
{
    [Key]
    [Column("user_id", Order = 1)]
    public int UserId { get; set; }

    [Key]
    [Column("id", Order = 2)]
    public int Id { get; set; }

    [Column("name")]
    [Required]
    [StringLength(500)]
    public string Name { get; set; }

    [Column("description")]
    [StringLength(4000)]
    public string Description { get; set; }

    [Column("parent_id")]
    public int? ParentId { get; set; }

    [Column("color")]
    [StringLength(100)]
    public string Color { get; set; }

    [Column("type_id")]
    public int TypeId { get; set; }

    [Column("order")]
    public int? Order { get; set; } 
}