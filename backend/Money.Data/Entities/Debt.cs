namespace Money.Data.Entities;

public class Debt : UserEntity
{
    /// <summary>
    /// Идентификатор типа.
    /// </summary>
    public int TypeId { get; set; }

    /// <summary>
    /// Сумма.
    /// </summary>
    public decimal Sum { get; set; }

    /// <summary>
    /// Комментарий.
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Дата возникновения.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Оплаченная сумма.
    /// </summary>
    public decimal PaySum { get; set; }

    /// <summary>
    /// Комментарии к оплате.
    /// </summary>
    public string? PayComment { get; set; }

    /// <summary>
    /// Идентификатор статуса.
    /// </summary>
    public int StatusId { get; set; }

    /// <summary>
    /// Идентификатор держателя.
    /// </summary>
    public int OwnerId { get; set; }

    /// <summary>
    /// Держатель.
    /// </summary>
    public DebtOwner? Owner { get; set; }

    /// <summary>
    /// Удален.
    /// </summary>
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
