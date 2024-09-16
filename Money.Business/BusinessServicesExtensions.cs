using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Money.Business.Interfaces;
using Money.Business.Services;

namespace Money.Business;

public static class BusinessServicesExtensions
{
    public static IServiceCollection ConfigureBusinessServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IAuthService, AuthService>();
        return services;
    }
}
