using Money.Api.Definitions.Base;

namespace Money.Api.Definitions;

public class AuthorizationDefinition : AppDefinition
{
    public override void ConfigureApplication(WebApplication app)
    {
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
    }
}
