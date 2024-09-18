using Money.Api.Definitions.Base;
using Money.Api.Middlewares;

namespace Money.Api.Definitions;

public class MiddlewareDefinition : AppDefinition
{
    public override int ApplicationOrderIndex => 1;

    public override void ConfigureApplication(IApplicationBuilder app)
    {
    }
}
