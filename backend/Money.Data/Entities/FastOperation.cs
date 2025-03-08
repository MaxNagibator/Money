namespace Money.Data.Entities;

/// <summary>
/// Быстрая операция.
/// </summary>
public class FastOperation : OperationBase
{
    /// <summary>
    /// Наименование.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Значение сортировки.
    /// </summary>
    public int? Order { get; set; }
}

public class FastOperationConfiguration : OperationBaseConfiguration<FastOperation>
{
    protected override void AddCustomConfiguration(EntityTypeBuilder<FastOperation> builder)
    {
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.Order)
            .IsRequired(false);
    }
}
