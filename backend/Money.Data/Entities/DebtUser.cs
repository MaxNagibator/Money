namespace Money.Data.Entities;

public class DebtUser : UserEntity
{
    public required string Name { get; set; }

    public List<Debt>? Debts { get; set; }
}

public class DebtUserConfiguration : UserEntityConfiguration<DebtUser>
{
    protected override void AddBaseConfiguration(EntityTypeBuilder<DebtUser> builder)
    {
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasMany(x => x.Debts)
            .WithOne(x => x.DebtUser)
            .HasForeignKey(x => new { x.UserId, x.DebtUserId })
            .IsRequired();
    }
}
