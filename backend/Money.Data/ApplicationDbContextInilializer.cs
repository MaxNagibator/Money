using Microsoft.Extensions.DependencyInjection;

namespace Money.Data;

public static class ApplicationDbContextInitializer
{
    public static IServiceProvider InitializeDatabaseContext(this IServiceProvider serviceProvider)
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        using ApplicationDbContext applicationDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        applicationDbContext.Database.Migrate();

        return serviceProvider;
    }
}
