using Money.Api.Definitions.Base;
using Money.Business;
using Money.Business.Services;

namespace Money.Api.Definitions;

public class ContainerDefinition : AppDefinition
{
    public override void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<RequestEnvironment>();

        builder.Services.AddScoped<AccountService>();

        builder.Services.AddScoped<CategoryService>();
    }
}
