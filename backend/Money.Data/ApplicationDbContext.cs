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
    public DbSet<Car> Cars { get; set; } = null!;
    public DbSet<CarEvent> CarEvents { get; set; } = null!;


    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public override int SaveChanges()
    {
        var changedEntities = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added ||
                       e.State == EntityState.Modified ||
                       e.State == EntityState.Deleted)
            .ToList();

        // audit_event
        // id, user_id, date

        // audit_entities
        // id, entityType, entityKey, event_id
        // 1(car), guid1, now, 217

        // audit_entity_props
        // audit_id, prop_key, prop_old_value, prop_new_value
        foreach (var entry in changedEntities)
        {

            Console.WriteLine($"Entity: {entry.Entity.GetType().Name}, " +
                $"State: {entry.State}, " +
                $"Time: {DateTime.Now}");

            foreach (var prop in entry.Properties)
            {
                if (prop.OriginalValue?.ToString() != prop.CurrentValue?.ToString())
                {
                    Console.WriteLine(prop.Metadata.Name + " " + prop.OriginalValue + "->" + prop.CurrentValue);
                }
            }
        }

        return base.SaveChanges();
    }
}
