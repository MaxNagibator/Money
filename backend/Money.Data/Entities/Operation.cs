namespace Money.Data.Entities;

public class Operation : OperationBase
{
    /// <summary>
    /// Дата.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Идентификатор родительской регулярной задачи.
    /// </summary>
    /// <remarks>
    /// Не null, если операция создана регулярной задачей.
    /// </remarks>
    public int? CreatedTaskId { get; set; }
}

public class OperationConfiguration : OperationBaseConfiguration<Operation>
{
    protected override void AddCustomConfiguration(EntityTypeBuilder<Operation> builder)
    {
        builder.Property(x => x.Date)
            .HasConversion(time => time.Date, time => time.Date)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(x => x.CreatedTaskId)
            .IsRequired(false);
    }
}
