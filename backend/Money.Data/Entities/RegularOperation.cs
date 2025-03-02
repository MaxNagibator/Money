namespace Money.Data.Entities;

/// <summary>
/// Регулярная операция.
/// </summary>
public class RegularOperation : OperationBase
{
    /// <summary>
    /// Наименование.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Идентификатор типа временного шага.
    /// </summary>
    public int TimeTypeId { get; set; }

    /// <summary>
    /// Значение временного шага.
    /// </summary>
    public int? TimeValue { get; set; }

    /// <summary>
    /// Дата начала.
    /// </summary>
    public DateTime DateFrom { get; set; }

    /// <summary>
    /// Дата окончания.
    /// </summary>
    public DateTime? DateTo { get; set; }

    /// <summary>
    /// Время последнего запуска.
    /// </summary>
    public DateTime? RunTime { get; set; }
}

public class RegularOperationConfiguration : OperationBaseConfiguration<RegularOperation>
{
    protected override void AddCustomConfiguration(EntityTypeBuilder<RegularOperation> builder)
    {
        builder.Property(x => x.Name)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.TimeTypeId)
            .IsRequired();

        builder.Property(x => x.TimeValue)
            .IsRequired(false);

        builder.Property(x => x.DateFrom)
            .HasConversion(time => time.Date, time => time.Date)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(x => x.DateTo)
            .HasConversion(time => time!.Value.Date, time => time.Date)
            .HasColumnType("date")
            .IsRequired(false);

        // TODO: Подумать над конвертацией
        builder.Property(x => x.RunTime)
            .HasConversion(time => time.HasValue ? time.Value.Date : (DateTime?)null, time => time.HasValue ? time.Value.Date : null)
            .HasColumnType("date")
            .IsRequired(false);
    }
}
