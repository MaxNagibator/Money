using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Money.Api.Data;

public class ApplicationDbContext(DbContextOptions options) : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options)
{
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<User> DomainUsers { get; set; } = null!;
}
