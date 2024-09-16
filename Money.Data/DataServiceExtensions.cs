using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Money.Data;

public static class DataServiceExtensions
{
    public static IServiceCollection ConfigureDataServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString(nameof(ApplicationDbContext)));
            options.UseSnakeCaseNamingConvention();
            options.UseOpenIddict();
        });

        return services;
    }
}
