using Microsoft.Extensions.DependencyInjection;

namespace Money.Data
{
    public static class ApplicationDbContextInilializer
    {
        public static IServiceProvider InilializeDbContext(this IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            scope.ServiceProvider.GetRequiredService<ApplicationDbContext>()
                .Database.Migrate();

            return serviceProvider;
        }
    }
}
