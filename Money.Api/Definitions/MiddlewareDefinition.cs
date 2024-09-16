using Money.Api.Definitions.Base;
using Money.Api.Middlewares;

namespace Money.Api.Definitions;

public class MiddlewareDefinition : AppDefinition
{
    public override int ApplicationOrderIndex => 1;

    public override void ConfigureApplication(WebApplication app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseMiddleware<AuthMiddleware>();
    }
}
