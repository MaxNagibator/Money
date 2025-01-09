namespace Money.Data.Entities;

public class DomainUser
{
    public int Id { get; set; }

    public Guid AuthUserId { get; set; }

    // TODO: обработать конкурентные изменения
    public int NextCategoryId { get; set; }

    public int NextOperationId { get; set; }

    public int NextPlaceId { get; set; }

    public int NextFastOperationId { get; set; }

    public int NextRegularOperationId { get; set; }

    public int NextDebtId { get; set; }

    public int NextDebtOwnerId { get; set; }

    public byte[]? RowVersion { get; set; }
}

public class DomainUserConfiguration : IEntityTypeConfiguration<DomainUser>
{
    public void Configure(EntityTypeBuilder<DomainUser> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd()
            .IsRequired();

        builder.Property(x => x.AuthUserId)
            .IsRequired();

        builder.Property(x => x.NextCategoryId)
            .HasDefaultValue(1)
            .IsRequired();

        builder.Property(x => x.NextOperationId)
            .HasDefaultValue(1)
            .IsRequired();

        builder.Property(x => x.NextPlaceId)
            .HasDefaultValue(1)
            .IsRequired();

        builder.Property(x => x.NextFastOperationId)
            .HasDefaultValue(1)
            .IsRequired();

        builder.Property(x => x.NextRegularOperationId)
            .HasDefaultValue(1)
            .IsRequired();

        builder.Property(x => x.NextDebtId)
            .HasDefaultValue(1)
            .IsRequired();

        builder.Property(x => x.NextDebtOwnerId)
            .HasDefaultValue(1)
            .IsRequired();

        builder.Property(x => x.RowVersion)
            .IsRowVersion()
            .IsRequired();
    }
}
