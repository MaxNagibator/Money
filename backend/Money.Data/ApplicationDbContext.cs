using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Money.Data.Entities;

namespace Money.Data;

public class ApplicationDbContext(DbContextOptions options) : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options)
{
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<DomainUser> DomainUsers { get; set; } = null!;
    public DbSet<Payment> Payments { get; set; } = null!;
    public DbSet<Place> Places { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Payment>().Property(e => e.Date).HasColumnType("date");

        builder.Entity<Category>().HasQueryFilter(x => x.IsDeleted == false);
        builder.Entity<Payment>().HasQueryFilter(x => x.IsDeleted == false);
        builder.Entity<Place>().HasQueryFilter(x => x.IsDeleted == false);
    }
}
