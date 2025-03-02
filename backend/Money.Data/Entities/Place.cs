namespace Money.Data.Entities;

/// <summary>
/// Место операции.
/// </summary>
public class Place : UserEntity
{
    /// <summary>
    /// Наименование.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Дата последнего использования.
    /// </summary>
    public DateTime LastUsedDate { get; set; }

    /// <summary>
    /// Удалено.
    /// </summary>
    public bool IsDeleted { get; set; }
}

public class PlaceConfiguration : UserEntityConfiguration<Place>
{
    protected override void AddBaseConfiguration(EntityTypeBuilder<Place> builder)
    {
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.LastUsedDate)
            .IsRequired();

        builder.Property(x => x.IsDeleted)
            .IsRequired();
    }
}
