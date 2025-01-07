using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Money.Data.Entities;
using System.Reflection;

namespace Money.Data;

public class ApplicationDbContext(DbContextOptions options) : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options)
{
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<DomainUser> DomainUsers { get; set; } = null!;
    public DbSet<Operation> Operations { get; set; } = null!;
    public DbSet<FastOperation> FastOperations { get; set; } = null!;
    public DbSet<RegularOperation> RegularOperations { get; set; } = null!;
    public DbSet<Place> Places { get; set; } = null!;
    public DbSet<Debt> Debts { get; set; } = null!;
    public DbSet<DebtOwner> DebtOwners { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
