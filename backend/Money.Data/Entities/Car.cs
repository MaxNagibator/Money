namespace Money.Data.Entities;

/// <summary>
/// Авто.
/// </summary>
public class Car : UserEntity
{
    /// <summary>
    /// Наименование.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Удалена.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Список авто-событий.
    /// </summary>
    public List<CarEvent>? Events { get; set; }
}

public class CarConfiguration : UserEntityConfiguration<Car>
{
    protected override void AddBaseConfiguration(EntityTypeBuilder<Car> builder)
    {
        builder.Property(x => x.Name)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(x => x.IsDeleted)
            .HasDefaultValue(false)
            .IsRequired();

        builder.HasMany(x => x.Events)
            .WithOne(x => x.Car)
            .HasForeignKey(x => new { x.UserId, x.CarId })
            .IsRequired();

        builder.HasQueryFilter(x => x.IsDeleted == false);
    }
}
