namespace Money.Data.Entities;

/// <summary>
/// Движение по долговому счёту.
/// </summary>
public class DebtMovement : UserEntity
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

public class DebtMovementConfiguration : UserEntityConfiguration<DebtMovement>
{
    protected override void AddBaseConfiguration(EntityTypeBuilder<DebtMovement> builder)
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

        builder.Property(x => x.IsDeleted)
            .IsRequired();

        builder.HasQueryFilter(x => x.IsDeleted == false);
    }
}
