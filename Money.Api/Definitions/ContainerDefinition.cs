using Money.Api.Definitions.Base;
using Money.BusinessLogic.Services;

namespace Money.Api.Definitions;

public class ContainerDefinition : AppDefinition
{
    public override void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<AccountService>();
        builder.Services.AddScoped<AuthService>();
    }
}
