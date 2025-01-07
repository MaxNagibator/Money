namespace Money.Data.Entities;

public class Place : UserEntity
{
    public required string Name { get; set; }

    public DateTime LastUsedDate { get; set; }

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
