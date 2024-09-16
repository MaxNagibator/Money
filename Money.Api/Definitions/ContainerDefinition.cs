using Money.Api.Definitions.Base;
using Money.Business.Interfaces;
using Money.Business.Services;

namespace Money.Api.Definitions;

public class ContainerDefinition : AppDefinition
{
    public override void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IAccountService, AccountService>();
        builder.Services.AddScoped<IAuthService, AuthService>();
    }
}
