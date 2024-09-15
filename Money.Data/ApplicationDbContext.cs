using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Money.Data.Entities;

namespace Money.Data;

public class ApplicationDbContext(DbContextOptions options) : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options)
{
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<DomainUser> DomainUsers { get; set; } = null!;
}
