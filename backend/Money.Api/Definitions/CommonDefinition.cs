using Money.Api.Definitions.Base;

namespace Money.Api.Definitions;

public class CommonDefinition : AppDefinition
{
    public override void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();

        builder.Services.AddLocalization();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddResponseCaching();
        builder.Services.AddMemoryCache();
    }

    public override void ConfigureApplication(WebApplication app)
    {
        app.MapControllers();
        app.MapDefaultControllerRoute();
    }
}
