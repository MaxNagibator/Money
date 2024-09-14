using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Money.Data.Interfaces;
using Money.Data.Repositories;

namespace Money.Data
{
    public static class DataServiceExtensions
    {
        public static IServiceCollection ConfigureDataServices(this IServiceCollection services, IConfiguration configuration)
        {
            var serviceProvider = services.BuildServiceProvider();
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseLoggerFactory(loggerFactory);
                options.UseSnakeCaseNamingConvention();
                options.UseNpgsql(configuration.GetConnectionString("SecurityDb"));
                options.UseOpenIddict();
            });

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            return services;
        }
    }
}
