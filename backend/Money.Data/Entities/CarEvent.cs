namespace Money.Data.Entities;

public class CarEvent : UserEntity
{
    public int CarId { get; set; }

    public int TypeId { get; set; }

    public string? Title { get; set; }

    public string? Comment { get; set; }

    public int? Mileage { get; set; }

    public DateTime Date { get; set; }

    public bool IsDeleted { get; set; }
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
