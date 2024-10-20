namespace Money.Data.Entities;

public class DomainCategory : UserEntity
{
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

    public bool IsDeleted { get; set; }

    public virtual DomainCategory? ParentCategory { get; set; }
    public virtual List<DomainCategory>? SubCategories { get; set; }
}
