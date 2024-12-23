using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Money.Data.Entities;

namespace Money.Data;

public class ApplicationDbContext(DbContextOptions options) : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options)
{
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<DomainUser> DomainUsers { get; set; } = null!;
    public DbSet<Operation> Operations { get; set; } = null!;
    public DbSet<FastOperation> FastOperations { get; set; } = null!;
    public DbSet<RegularOperation> RegularOperations { get; set; } = null!;
    public DbSet<Place> Places { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Operation>()
            .Property(e => e.Date)
            .HasConversion(time => time.Date, time => time.Date)
            .HasColumnType("date");

        builder.Entity<Category>()
            .HasMany(c => c.SubCategories)
            .WithOne(c => c.ParentCategory)
            .HasForeignKey(nameof(Category.UserId), nameof(Category.ParentId));

        builder.Entity<Category>().HasQueryFilter(x => x.IsDeleted == false);
        builder.Entity<Operation>().HasQueryFilter(x => x.IsDeleted == false);
        builder.Entity<FastOperation>().HasQueryFilter(x => x.IsDeleted == false);
        builder.Entity<RegularOperation>().HasQueryFilter(x => x.IsDeleted == false);

        builder.Entity<RegularOperation>()
            .Property(e => e.DateFrom)
            .HasColumnType("date");

        builder.Entity<RegularOperation>()
            .Property(e => e.DateTo)
            .HasColumnType("date");

        builder.Entity<RegularOperation>()
            .Property(e => e.RunTime)
            .HasColumnType("date");
    }
}
