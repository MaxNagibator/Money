namespace Money.Data.Entities;

/// <summary>
/// Авто-событие.
/// </summary>
public class CarEvent : UserEntity
{
    /// <summary>
    /// Идентификатор связанного автомобиля.
    /// </summary>
    public int CarId { get; set; }

    /// <summary>
    /// Идентификатор типа.
    /// </summary>
    public int TypeId { get; set; }

    /// <summary>
    /// Название.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Дополнительные комментарии.
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Пробег автомобиля.
    /// </summary>
    public int? Mileage { get; set; }

    /// <summary>
    /// Дата.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Удалено.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Связанный автомобиль.
    /// </summary>
    public Car? Car { get; set; }
}

public class CarEventConfiguration : UserEntityConfiguration<CarEvent>
{
    protected override void AddBaseConfiguration(EntityTypeBuilder<CarEvent> builder)
    {
        builder.Property(x => x.CarId)
            .IsRequired();

        builder.Property(x => x.TypeId)
            .IsRequired();

        builder.Property(x => x.Title)
            .HasMaxLength(1000)
            .IsRequired(false);

        builder.Property(x => x.Comment)
            .IsRequired(false);

        builder.Property(x => x.Mileage)
            .IsRequired(false);

        builder.Property(x => x.Date)
            .HasConversion(time => time.Date, time => time.Date)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(x => x.IsDeleted)
            .HasDefaultValue(false)
            .IsRequired();

        builder.HasQueryFilter(x => x.IsDeleted == false);
    }
}
