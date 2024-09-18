using Money.Api.Definitions.Base;
using Money.Api.Middlewares;

namespace Money.Api.Definitions;

public class AuthorizationDefinition : AppDefinition
{
    public override void ConfigureApplication(IApplicationBuilder app)
    {
        app.UseHttpsRedirection();

        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseMiddleware<AuthMiddleware>();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapDefaultControllerRoute();
        });
    }
}
