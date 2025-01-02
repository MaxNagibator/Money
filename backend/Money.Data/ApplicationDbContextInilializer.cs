using Microsoft.Extensions.DependencyInjection;

namespace Money.Data;

public static class ApplicationDbContextInitializer
{
    public static IServiceProvider InitializeDatabaseContext(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        using var applicationDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        applicationDbContext.Database.Migrate();

        return serviceProvider;
    }
}
