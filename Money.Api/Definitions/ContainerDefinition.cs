using Money.Api.Definitions.Base;
using Money.Business;
using Money.Business.Services;

namespace Money.Api.Definitions;

public class ContainerDefinition : AppDefinition
{
    public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<RequestEnvironment>();

        services.AddScoped<AccountService>();
        services.AddScoped<AuthService>();

        services.AddScoped<PaymentCategoryService>();
    }
}
