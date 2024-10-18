namespace Money.Data.Entities;

public class DomainPlace : UserEntity
{
    [StringLength(500)]
    public required string Name { get; set; }

    [StringLength(4000)]
    public string? Description { get; set; }

    public DateTime? LastUsedDate { get; set; }
    public bool IsDeleted { get; set; }
}
