namespace Money.Data.Entities;

public partial class Car : UserEntity
{
    public required string Name { get; set; }

    public bool IsDeleted { get; set; }
}

public class CarConfiguration : UserEntityConfiguration<Car>
{
    protected override void AddBaseConfiguration(EntityTypeBuilder<Car> builder)
    {
        builder.Property(x => x.Name)
            .HasMaxLength(1000)
            .IsRequired();

        builder.HasQueryFilter(x => x.IsDeleted == false);
    }
}

