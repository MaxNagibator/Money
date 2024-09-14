using Money.Api.Data;
using Money.BusinessLogic;
using Money.Data;
using Microsoft.AspNetCore.Identity;
using Money.Api.Definitions;
using Serilog;
using Money.Common.Logging;

namespace Money.Api.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddSerilog();
            });

            services.AddSingleton<ILoggerFactory>(_ => SerilogFactory.InitLogging());
            services.ConfigureBusinessLogicServices(configuration);
            services.ConfigureDataServices(configuration);

            services.AddIdentity<ApplicationUser, ApplicationRole>()
                    .AddEntityFrameworkStores<ApplicationDbContext>()
                    .AddDefaultTokenProviders();

            services.AddSwaggerDefinition();
            services.AddOpenIddictDefinition();
            return services;
        }
    }
}
