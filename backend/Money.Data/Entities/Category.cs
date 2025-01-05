namespace Money.Data.Entities;

public class Category : UserEntity
{
    public required string Name { get; set; }

    public string? Description { get; set; }

    public int? ParentId { get; set; }

    public string? Color { get; set; }

    public int TypeId { get; set; }

    public int? Order { get; set; }

    public bool IsDeleted { get; set; }

    public virtual Category? ParentCategory { get; set; }
    public virtual List<Category>? SubCategories { get; set; }
}

public class CategoryConfiguration : UserEntityConfiguration<Category>
{
    protected override void AddBaseConfiguration(EntityTypeBuilder<Category> builder)
    {
        builder.Property(x => x.Name)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(4000)
            .IsRequired(false);

        builder.Property(x => x.ParentId)
            .IsRequired(false);

        builder.Property(x => x.Color)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(x => x.TypeId)
            .IsRequired();

        builder.Property(x => x.Order)
            .IsRequired(false);

        builder.Property(x => x.IsDeleted)
            .IsRequired();

        builder.HasMany(x => x.SubCategories)
            .WithOne(x => x.ParentCategory)
            .HasForeignKey(x => new { x.UserId, x.ParentId })
            .IsRequired(false);

        builder.HasQueryFilter(x => x.IsDeleted == false);
    }
}
