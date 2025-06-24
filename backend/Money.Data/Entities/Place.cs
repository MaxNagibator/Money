using System.Linq.Expressions;

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
    private readonly Expression<Func<DateTime, DateTime>> _convertToUtc = dateTime => dateTime.Kind == DateTimeKind.Utc
        ? dateTime
        : DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);

    protected override void AddBaseConfiguration(EntityTypeBuilder<Place> builder)
    {
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(500)
            .HasColumnType("citext");

        builder.Property(x => x.LastUsedDate).HasConversion(_convertToUtc, _convertToUtc).IsRequired();

        builder.Property(x => x.IsDeleted)
            .IsRequired();

        builder.HasIndex(x => x.Name);
    }
}
