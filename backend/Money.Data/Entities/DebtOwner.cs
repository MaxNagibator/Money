namespace Money.Data.Entities;

public class DebtOwner : UserEntity
{
    /// <summary>
    /// Имя держателя долга.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Список принадлежащих долгов.
    /// </summary>
    public List<Debt>? Debts { get; set; }
}

public class DebtOwnerConfiguration : UserEntityConfiguration<DebtOwner>
{
    protected override void AddBaseConfiguration(EntityTypeBuilder<DebtOwner> builder)
    {
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasMany(x => x.Debts)
            .WithOne(x => x.Owner)
            .HasForeignKey(x => new { x.UserId, DebtOwnerId = x.OwnerId })
            .IsRequired();
    }
}
