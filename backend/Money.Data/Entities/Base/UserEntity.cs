namespace Money.Data.Entities.Base;

/// <summary>
/// Сущность принадлежащая пользователю.
/// </summary>
public abstract class UserEntity
{
    /// <summary>
    /// Идентификатор пользователя.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Идентификатор.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Пользователь, которому принадлежит.
    /// </summary>
    public DomainUser? User { get; set; }
}

public abstract class UserEntityConfiguration<T> : IEntityTypeConfiguration<T> where T : UserEntity
{
    public void Configure(EntityTypeBuilder<T> builder)
    {
        builder.HasKey(x => new { x.UserId, x.Id });

        builder.Property(x => x.UserId)
            .HasColumnOrder(1)
            .IsRequired();

        builder.Property(x => x.Id)
            .HasColumnOrder(2)
            .IsRequired();

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId);

        AddBaseConfiguration(builder);
    }

    protected abstract void AddBaseConfiguration(EntityTypeBuilder<T> builder);
}
