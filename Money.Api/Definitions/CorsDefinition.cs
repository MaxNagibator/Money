using Money.Api.Definitions.Base;

namespace Money.Api.Definitions;

public class CorsDefinition : AppDefinition
{
    public override void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.AddCors();
    }
}
