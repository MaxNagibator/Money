﻿namespace Money.Data.Entities.Base;

/// <summary>
/// Базовая операция.
/// </summary>
public abstract class OperationBase : UserEntity
{
    /// <summary>
    /// Сумма.
    /// </summary>
    public decimal Sum { get; set; }

    /// <summary>
    /// Идентификатор категории.
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// Комментарий.
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Идентификатор места.
    /// </summary>
    public int? PlaceId { get; set; }

    /// <summary>
    /// Удалена.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Категория.
    /// </summary>
    public Category? Category { get; set; }
}

public abstract class OperationBaseConfiguration<T> : UserEntityConfiguration<T> where T : OperationBase
{
    protected sealed override void AddBaseConfiguration(EntityTypeBuilder<T> builder)
    {
        builder.Property(x => x.Sum)
            .IsRequired();

        builder.Property(x => x.Comment)
            .HasMaxLength(4000)
            .IsRequired(false);

        builder.Property(x => x.PlaceId)
            .IsRequired(false);

        builder.Property(x => x.IsDeleted)
            .IsRequired();

        builder.HasOne(x => x.Category)
            .WithMany()
            .HasForeignKey(x => new { x.UserId, x.CategoryId })
            .IsRequired();

        builder.HasQueryFilter(x => x.IsDeleted == false);

        AddCustomConfiguration(builder);
    }

    protected abstract void AddCustomConfiguration(EntityTypeBuilder<T> builder);
}
