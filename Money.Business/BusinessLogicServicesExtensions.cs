using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Money.BusinessLogic.Interfaces;
using Money.BusinessLogic.Services;

namespace Money.BusinessLogic
{
    public static class BusinessLogicServicesExtensions
    {
        public static IServiceCollection ConfigureBusinessLogicServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IAuthService, AuthService>();
            return services;
        }
    }
}
