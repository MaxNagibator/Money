using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Money.Data.Entities;

namespace Money.Data;

public class ApplicationDbContext(DbContextOptions options) : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options)
{
    public DbSet<DomainCategory> Categories { get; set; } = null!;
    public DbSet<DomainUser> DomainUsers { get; set; } = null!;
    public DbSet<DomainOperation> Operations { get; set; } = null!;
    public DbSet<DomainPlace> Places { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<DomainOperation>()
            .Property(e => e.Date)
            .HasConversion(time => time.Date, time => time.Date)
            .HasColumnType("date");

        builder.Entity<DomainCategory>()
            .HasMany(c => c.SubCategories)
            .WithOne(c => c.ParentCategory)
            .HasForeignKey(nameof(DomainCategory.UserId), nameof(DomainCategory.ParentId));

        builder.Entity<DomainCategory>().HasQueryFilter(x => x.IsDeleted == false);
        builder.Entity<DomainOperation>().HasQueryFilter(x => x.IsDeleted == false);
    }
}
