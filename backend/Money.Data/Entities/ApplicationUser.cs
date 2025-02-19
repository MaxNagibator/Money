using Microsoft.AspNetCore.Identity;

namespace Money.Data.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public string? EmailConfirmCode { get; set; }
}

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(x => x.EmailConfirmCode)
            .HasMaxLength(64)
            .IsRequired(false);
    }
}
