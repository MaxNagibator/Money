namespace Money.Data.Entities;

public class Debt : UserEntity
{
    public DateTime Date { get; set; }

    public decimal Sum { get; set; }

    public int TypeId { get; set; }

    public string? Comment { get; set; }

    public decimal PaySum { get; set; }

    public int StatusId { get; set; }

    public string? PayComment { get; set; }

    public int DebtUserId { get; set; }
    public DebtUser? DebtUser { get; set; }

    public bool IsDeleted { get; set; }
}

public class DebtConfiguration : UserEntityConfiguration<Debt>
{
    protected override void AddBaseConfiguration(EntityTypeBuilder<Debt> builder)
    {
        builder.Property(x => x.Date)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(x => x.Sum)
            .IsRequired();

        builder.Property(x => x.TypeId)
            .IsRequired();

        builder.Property(x => x.Comment)
            .HasMaxLength(4000)
            .IsRequired(false);

        builder.Property(x => x.PaySum)
            .IsRequired();

        builder.Property(x => x.StatusId)
            .IsRequired();

        builder.Property(x => x.PayComment)
            .HasMaxLength(4000)
            .IsRequired(false);

        builder.Property(x => x.IsDeleted)
            .IsRequired();

        builder.HasQueryFilter(x => x.IsDeleted == false);
    }
}
