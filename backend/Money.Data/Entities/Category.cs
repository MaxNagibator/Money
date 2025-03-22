namespace Money.Data.Entities;

/// <summary>
/// Категория операции.
/// </summary>
public class Category : UserEntity
{
    /// <summary>
    /// Наименование.
    /// </summary>
    public required string Name { get; set; }

    // TODO: Удалить на всех уровнях
    public string? Description { get; set; }

    /// <summary>
    /// Идентификатор родительской категории (если есть).
    /// </summary>
    public int? ParentId { get; set; }

    /// <summary>
    /// Цвет.
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// Идентификатор типа операции.
    /// </summary>
    public int TypeId { get; set; }

    /// <summary>
    /// Порядок отображения.
    /// </summary>
    public int? Order { get; set; }

    /// <summary>
    /// Удалена.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Родительская категория (если есть).
    /// </summary>
    public virtual Category? ParentCategory { get; set; }

    /// <summary>
    /// Дочерние категории.
    /// </summary>
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
